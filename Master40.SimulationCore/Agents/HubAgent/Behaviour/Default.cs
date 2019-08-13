using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using static FArticles;
using static FBreakDowns;
using static FHubInformations;
using static FOperationResults;
using static FOperations;
using static FProposals;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class Default : MessageTypes.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                        : base(null, simulationType) { }


        internal List<FOperation> operationList { get; set; } = new List<FOperation>();
        internal AgentDictionary resourceAgents { get; set; } = new AgentDictionary();

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Hub.Instruction.EnqueueJob msg: EnqueueJob(agent, msg.GetObjectFromMessage as FOperation); break;
                case Hub.Instruction.ProductionStarted msg: ProductionStarted(agent, msg.GetObjectfromMessage); break;
                case Hub.Instruction.FinishJob msg: FinishJob(agent, msg.GetObjectFromMessage as FOperationResult); break;
                case Hub.Instruction.ProposalFromMachine msg: ProposalFromMachine(agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.AddMachineToHub msg: AddResourceToHub(agent, msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown(agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ResourceBreakDown(Agent agent, FBreakDown breakDown)
        {
            var brockenMachine = resourceAgents.Single(x => breakDown.Resource == x.Value).Key;
            resourceAgents.Remove(brockenMachine);
            agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakDown, brockenMachine, true), 45);

            System.Diagnostics.Debug.WriteLine("Break for " + breakDown.Resource, "Hub");
        }

        private void EnqueueJob(Agent agent, FOperation workItem)
        {
            var localItem = operationList.Find(x => x.Key == workItem.Key);
            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (localItem == null)
            {
                // Set Comunication agent.
                localItem = workItem.UpdateHubAgent(agent.Context.Self);
                // add TO queue
                operationList.Add(localItem);
                agent.DebugMessage("Got New Item to Enqueue: " + workItem.Operation.Name + " | with start condition:" + workItem.StartConditions.Satisfied + " with Id: " + workItem.Key);
            }
            else
            {
                // reset Item.
                agent.DebugMessage("Got Item to Requeue: " + workItem.Operation.Name + " | with start condition:" + workItem.StartConditions.Satisfied + " with Id: " + workItem.Key);
                workItem.Proposals.Clear();
                localItem = workItem.UpdateHubAgent(agent.Context.Self);
                operationList.Replace(localItem);
            }

            foreach (var actorRef in resourceAgents)
            {
                agent.Send(instruction: Resource.Instruction.RequestProposal.Create(localItem, actorRef.Key));
            }
        }

        public void ProductionStarted(Agent agent, Guid key)
        {
            var temp = operationList.Single(x => x.Key == key);
            agent.Send(Production.Instruction
                                 .ProductionStarted
                                 .Create(message: temp.Key
                                        , target: temp.ProductionAgent));
            operationList.Replace(temp);
        }

        public void FinishJob(Agent agent, FOperationResult operationResult)
        {
            var item = operationList.Find(x => x.Key == operationResult.Key);

            agent.DebugMessage("Machine called Item " + item.Operation.Name + " with Id: " + operationResult.Key + " finished.");
            agent.Send(Production.Instruction.FinishWorkItem.Create(operationResult, item.ProductionAgent));
            operationList.Remove(item);
        }

        /// <summary>
        /// TODO: Has to be Rewitten
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="proposal"></param>
        private void ProposalFromMachine(Agent agent, FProposal proposal)
        {
            // get releated workitem and add Proposal.
            var operation = operationList.SingleOrDefault(x => x.Key == proposal.JobKey);
            operation.Proposals.RemoveAll(x => x.ResourceAgent == proposal.ResourceAgent);
            // add New Proposal
            operation.Proposals.Add(proposal);

            agent.DebugMessage("Proposal for Schedule: " + proposal.PossibleSchedule + " Id: " + proposal.JobKey + " from:" + proposal.ResourceAgent + "!");


            // if all Machines Answered
            if (operation.Proposals.Count == resourceAgents.Count)
            {

                // item Postponed by All Machines ? -> reque after given amount of time.
                if (operation.Proposals.TrueForAll(x => x.Postponed.IsPostponed))
                {
                    // Call Hub Agent to Requeue
                    operation = operation.UpdateResourceAgent(ActorRefs.NoSender);
                    operationList.Replace(operation);
                    agent.Send(Hub.Instruction.EnqueueJob.Create(operation, agent.Context.Self), proposal.Postponed.Offset);
                    return;
                }


                // aknowledge Machine -> therefore get Machine -> send aknowledgement
                var acknowledgement = operation.Proposals.First(x => x.PossibleSchedule == operation.Proposals.Where(y => y.Postponed.IsPostponed == false)
                                                                                                            .Min(p => p.PossibleSchedule)
                                                                 && x.Postponed.IsPostponed == false);

                agent.DebugMessage("Start AcknowledgeProposal for " + proposal.JobKey + " on resource " + acknowledgement.ResourceAgent);

                // set Proposal Start for Machine to Reque if time slot is closed.
                operationList.Replace(operation);
                agent.Send(Resource.Instruction.AcknowledgeProposal.Create(operation, acknowledgement.ResourceAgent));
            }
        }


        private void AddResourceToHub(Agent agent, FHubInformation hubInformation)
        {
            // Add Machine to Pool
            resourceAgents.Add(hubInformation.Ref, hubInformation.RequiredFor);
            // Added Machine Agent To Machine Pool
            agent.DebugMessage("Added Machine Agent " + hubInformation.Ref.Path.Name + " to Machine Pool: " + hubInformation.RequiredFor);
        }
    }
}
