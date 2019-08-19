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
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                        : base(childMaker: null, obj: simulationType) { }


        internal List<FOperation> _operationList { get; set; } = new List<FOperation>();
        internal AgentDictionary _resourceAgents { get; set; } = new AgentDictionary();

        public override bool Action(object message)
        {
            switch (message)
            {
                case Hub.Instruction.EnqueueJob msg: EnqueueJob(fOperation: msg.GetObjectFromMessage as FOperation); break;
                case Hub.Instruction.ProductionStarted msg: ProductionStarted(key: msg.GetObjectfromMessage); break;
                case Hub.Instruction.FinishJob msg: FinishJob(operationResult: msg.GetObjectFromMessage as FOperationResult); break;
                case Hub.Instruction.ProposalFromMachine msg: ProposalFromMachine(proposal: msg.GetObjectFromMessage); break;
                case Hub.Instruction.AddResourceToHub msg: AddResourceToHub(hubInformation: msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown(breakDown: msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ResourceBreakDown(FBreakDown breakDown)
        {
            var brockenMachine = _resourceAgents.Single(predicate: x => breakDown.Resource == x.Value).Key;
            _resourceAgents.Remove(key: brockenMachine);
            Agent.Send(instruction: BasicInstruction.ResourceBrakeDown.Create(message: breakDown, target: brockenMachine, logThis: true), waitFor: 45);

            System.Diagnostics.Debug.WriteLine(message: "Break for " + breakDown.Resource, category: "Hub");
        }

        private void EnqueueJob(FOperation fOperation)
        {
            var localItem = _operationList.Find(match: x => x.Key == fOperation.Key);
            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (localItem == null)
            {
                localItem = fOperation.UpdateHubAgent(hub: Agent.Context.Self);
                _operationList.Add(item: localItem);

                Agent.DebugMessage(msg: "Got New Item to Enqueue: " + fOperation.Operation.Name + " | with start condition:" + fOperation.StartConditions.Satisfied + " with Id: " + fOperation.Key);
            }
            else
            {
                // reset Item.
                Agent.DebugMessage(msg: "Got Item to Requeue: " + fOperation.Operation.Name + " | with start condition:" + fOperation.StartConditions.Satisfied + " with Id: " + fOperation.Key);
                fOperation.Proposals.Clear();
                //localItem = fOperation.UpdateHubAgent(hub: Agent.Context.Self);
                _operationList.Replace(val: localItem);
            }

            foreach (var actorRef in _resourceAgents)
            {
                Agent.Send(instruction: Resource.Instruction.RequestProposal.Create(message: localItem, target: actorRef.Key));
            }
        }

        public void ProductionStarted(Guid key)
        {
            var temp = _operationList.Single(predicate: x => x.Key == key);
            Agent.Send(instruction: Production.Instruction
                                 .ProductionStarted
                                 .Create(message: temp.Key
                                        , target: temp.ProductionAgent));
            _operationList.Replace(val: temp);
        }

        public void FinishJob(FOperationResult operationResult)
        {
            var item = _operationList.Find(match: x => x.Key == operationResult.Key);

            Agent.DebugMessage(msg: "Machine called Item " + item.Operation.Name + " with Id: " + operationResult.Key + " finished.");
            Agent.Send(instruction: Production.Instruction.FinishWorkItem.Create(message: operationResult, target: item.ProductionAgent));
            _operationList.Remove(item: item);
        }

        /// <summary>
        /// TODO: Has to be Rewitten
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="proposal"></param>
        private void ProposalFromMachine(FProposal proposal)
        {
            // get releated workitem and add Proposal.
            var operation = _operationList.SingleOrDefault(predicate: x => x.Key == proposal.JobKey);
            operation.Proposals.RemoveAll(match: x => x.ResourceAgent == proposal.ResourceAgent);
            // add New Proposal
            operation.Proposals.Add(item: proposal);

            Agent.DebugMessage(msg: "Proposal for Schedule: " + proposal.PossibleSchedule + " Id: " + proposal.JobKey + " from:" + proposal.ResourceAgent + "!");


            // if all Machines Answered
            if (operation.Proposals.Count == _resourceAgents.Count)
            {

                // item Postponed by All Machines ? -> reque after given amount of time.
                if (operation.Proposals.TrueForAll(match: x => x.Postponed.IsPostponed))
                {
                    // Call Hub Agent to Requeue
                    operation = operation.UpdateResourceAgent(r: ActorRefs.NoSender);
                    _operationList.Replace(val: operation);
                    Agent.Send(instruction: Hub.Instruction.EnqueueJob.Create(message: operation, target: Agent.Context.Self), waitFor: proposal.Postponed.Offset);
                    return;
                }


                // aknowledge Machine -> therefore get Machine -> send aknowledgement
                var acknowledgement = operation.Proposals.First(predicate: x => x.PossibleSchedule == operation.Proposals.Where(predicate: y => y.Postponed.IsPostponed == false)
                                                                                                            .Min(selector: p => p.PossibleSchedule)
                                                                 && x.Postponed.IsPostponed == false);

                // WTF
                operation = ((IJob)operation).SetEstimatedEnd(acknowledgement.PossibleSchedule) as FOperation;

                Agent.DebugMessage(msg: "Start AcknowledgeProposal for " + proposal.JobKey + " on resource " + acknowledgement.ResourceAgent);

                // set Proposal Start for Machine to Reque if time slot is closed.
                _operationList.Replace(val: operation);
                Agent.Send(instruction: Resource.Instruction.AcknowledgeProposal.Create(message: operation, target: acknowledgement.ResourceAgent));
            }
        }


        private void AddResourceToHub(FAgentInformation hubInformation)
        {
            // Add Machine to Pool
            _resourceAgents.Add(key: hubInformation.Ref, value: hubInformation.RequiredFor);
            // Added Machine Agent To Machine Pool
            Agent.DebugMessage(msg: "Added Machine Agent " + hubInformation.Ref.Path.Name + " to Machine Pool: " + hubInformation.RequiredFor);
        }
    }
}
