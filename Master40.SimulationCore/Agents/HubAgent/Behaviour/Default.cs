using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBreakDowns;
using static FAgentInformations;
using static FOperationResults;
using static FOperations;
using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                        : base(null, simulationType) { }


        internal List<FOperation> operationList { get; set; } = new List<FOperation>();
        internal AgentDictionary resourceAgents { get; set; } = new AgentDictionary();

        public override bool Action(object message)
        {
            switch (message)
            {
                case Hub.Instruction.EnqueueJob msg: EnqueueJob(msg.GetObjectFromMessage as FOperation); break;
                case Hub.Instruction.ProductionStarted msg: ProductionStarted(msg.GetObjectfromMessage); break;
                case Hub.Instruction.FinishJob msg: FinishJob(msg.GetObjectFromMessage as FOperationResult); break;
                case Hub.Instruction.ProposalFromMachine msg: ProposalFromMachine(msg.GetObjectFromMessage); break;
                case Hub.Instruction.AddMachineToHub msg: AddResourceToHub(msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown(msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ResourceBreakDown(FBreakDown breakDown)
        {
            var brockenMachine = resourceAgents.Single(x => breakDown.Resource == x.Value).Key;
            resourceAgents.Remove(brockenMachine);
            Agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakDown, brockenMachine, true), 45);

            System.Diagnostics.Debug.WriteLine("Break for " + breakDown.Resource, "Hub");
        }

        private void EnqueueJob(FOperation workItem)
        {
            var localItem = operationList.Find(x => x.Key == workItem.Key);
            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (localItem == null)
            {
                // Set Comunication Agent.
                localItem = workItem.UpdateHubAgent(Agent.Context.Self);
                // add TO queue
                operationList.Add(localItem);
                Agent.DebugMessage("Got New Item to Enqueue: " + workItem.Operation.Name + " | with start condition:" + workItem.StartConditions.Satisfied + " with Id: " + workItem.Key);
            }
            else
            {
                // reset Item.
                Agent.DebugMessage("Got Item to Requeue: " + workItem.Operation.Name + " | with start condition:" + workItem.StartConditions.Satisfied + " with Id: " + workItem.Key);
                workItem.Proposals.Clear();
                localItem = workItem.UpdateHubAgent(Agent.Context.Self);
                operationList.Replace(localItem);
            }

            foreach (var actorRef in resourceAgents)
            {
                Agent.Send(instruction: Resource.Instruction.RequestProposal.Create(localItem, actorRef.Key));
            }
        }

        public void ProductionStarted(Guid key)
        {
            var temp = operationList.Single(x => x.Key == key);
            Agent.Send(Production.Instruction
                                 .ProductionStarted
                                 .Create(message: temp.Key
                                        , target: temp.ProductionAgent));
            operationList.Replace(temp);
        }

        public void FinishJob(FOperationResult operationResult)
        {
            var item = operationList.Find(x => x.Key == operationResult.Key);

            Agent.DebugMessage("Machine called Item " + item.Operation.Name + " with Id: " + operationResult.Key + " finished.");
            Agent.Send(Production.Instruction.FinishWorkItem.Create(operationResult, item.ProductionAgent));
            operationList.Remove(item);
        }

        /// <summary>
        /// TODO: Has to be Rewitten
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="proposal"></param>
        private void ProposalFromMachine(FProposal proposal)
        {
            // get releated workitem and add Proposal.
            var operation = operationList.SingleOrDefault(x => x.Key == proposal.JobKey);
            operation.Proposals.RemoveAll(x => x.ResourceAgent == proposal.ResourceAgent);
            // add New Proposal
            operation.Proposals.Add(proposal);

            Agent.DebugMessage("Proposal for Schedule: " + proposal.PossibleSchedule + " Id: " + proposal.JobKey + " from:" + proposal.ResourceAgent + "!");


            // if all Machines Answered
            if (operation.Proposals.Count == resourceAgents.Count)
            {

                // item Postponed by All Machines ? -> reque after given amount of time.
                if (operation.Proposals.TrueForAll(x => x.Postponed.IsPostponed))
                {
                    // Call Hub Agent to Requeue
                    operation = operation.UpdateResourceAgent(ActorRefs.NoSender);
                    operationList.Replace(operation);
                    Agent.Send(Hub.Instruction.EnqueueJob.Create(operation, Agent.Context.Self), proposal.Postponed.Offset);
                    return;
                }


                // aknowledge Machine -> therefore get Machine -> send aknowledgement
                var acknowledgement = operation.Proposals.First(x => x.PossibleSchedule == operation.Proposals.Where(y => y.Postponed.IsPostponed == false)
                                                                                                            .Min(p => p.PossibleSchedule)
                                                                 && x.Postponed.IsPostponed == false);

                Agent.DebugMessage("Start AcknowledgeProposal for " + proposal.JobKey + " on resource " + acknowledgement.ResourceAgent);

                // set Proposal Start for Machine to Reque if time slot is closed.
                operationList.Replace(operation);
                Agent.Send(Resource.Instruction.AcknowledgeProposal.Create(operation, acknowledgement.ResourceAgent));
            }
        }


        private void AddResourceToHub(FAgentInformation hubInformation)
        {
            // Add Machine to Pool
            resourceAgents.Add(hubInformation.Ref, hubInformation.RequiredFor);
            // Added Machine Agent To Machine Pool
            Agent.DebugMessage("Added Machine Agent " + hubInformation.Ref.Path.Name + " to Machine Pool: " + hubInformation.RequiredFor);
        }
    }
}
