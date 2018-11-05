using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents
{
    /// <summary>
    /// Alternative Namen; ResourceAllocation, RessourceGroup, Mediator, Coordinator, Hub
    /// </summary>
    public partial class HubAgent : Agent
    {
        private List<WorkItem> _workItemQueue { get; }
        private List<IActorRef> _machineAgents { get; }
        private string _skillGroup { get; } 

        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time,string skillGroup, bool debug)
        {
            return Akka.Actor.Props.Create(() => new HubAgent(actorPaths, time, skillGroup, debug));
        }

        public HubAgent(ActorPaths actorPaths, long time,string skillGroup, bool debug) : base(actorPaths, time, debug)
        {
            _skillGroup = skillGroup;
            _workItemQueue = new List<WorkItem>();
            _machineAgents = new List<IActorRef>();
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case Instruction.EnqueueWorkItem instruction: EnqueueWorkItem(instruction.GetObjectFromMessage); break;
                case Instruction.AddMachineToHub instruction: AddMachineToHub(instruction); break;
                case Instruction.ProposalFromMachine instruction: ProposalFromMachine(instruction.GetObjectFromMessage); break;
                case Instruction.SetWorkItemStatus instruction: SetWorkItemStatus(instruction.GetObjectFromMessage); break;
                case Instruction.FinishWorkItem instruction: FinishWorkItem(instruction.GetObjectFromMessage); break;
                case Instruction.ProductionStarted instruction: ForwardProductionStart(instruction.GetObjectfromMessage); break;
                default: throw new Exception("Invalid Message Object.");
            }
        }

        private void FinishWorkItem(WorkItem workItem)
        {
            
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            DebugMessage("Machine called " + workItem.WorkSchedule.Name + " finished.");
            CreateAndEnqueue(ProductionAgent.Instruction.FinishWorkItem.Create(workItem, workItem.ProductionAgent));

            _workItemQueue.Remove(_workItemQueue.Find(x => x.Key == workItem.Key));
        }

        private void ForwardProductionStart(WorkItem workItem)
        {
            var temp = _workItemQueue.Single(x => x.Key == workItem.Key);
            CreateAndEnqueue(ProductionAgent.Instruction.ProductionStarted.Create(workItem.UpdatePoductionAgent(temp.ProductionAgent), temp.ProductionAgent));

        }

        private void SetWorkItemStatus(ItemStatus workItemStatus)
        {
            if (workItemStatus == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            // update Status
            var workItem = _workItemQueue.Find(x => x.Key == workItemStatus.ItemId)
                                         .UpdateStatus(workItemStatus.Status);
            
            DebugMessage("Set Item: " + workItem.WorkSchedule.Name + " | Status to: " + workItem.Status);
            // if 
            if (workItem.Status == ElementStatus.Ready)
            {
                // Call for Work 
                workItem = workItem.UpdateMaterialsProvided(true).SetReady;

                // Check if this item has a corrosponding mashine slot
                if (workItem.ResourceAgent == ActorRefs.NoSender)
                {
                    // If not Call Comunication Agent to Requeue
                    // do Nothing 
                    // CreateAndEnqueueInstuction(methodName: ComunicationAgent.InstuctionsMethods.EnqueueWorkItem.ToString(),
                    //     objectToProcess: workItem,
                    //     targetAgent: workItem.ComunicationAgent);
                }
                else
                {
                    CreateAndEnqueue(ResourceAgent.Instruction.StartWorkWith.Create(workItemStatus, workItem.ResourceAgent));
                    DebugMessage("Call for Work");
                }
            }
            _workItemQueue.Replace(workItem);
        }

        private void ProposalFromMachine(Proposal proposal)
        {
            if (proposal == null)
            {
                throw new InvalidCastException("Could not Cast Proposal on InstructionSet.ObjectToProcess");
            }
            // get releated workitem and add Proposal.
            var workItem = _workItemQueue.Find(x => x.Key == proposal.WorkItemId);
            var proposalToRemove = workItem.Proposals.Find(x => x.ResourceAgent == proposal.ResourceAgent );
            if (proposalToRemove != null)
            {
                workItem.Proposals.Remove(proposalToRemove);
            }

            workItem.Proposals.Add(proposal);

            DebugMessage("Proposal for Schedule: " + proposal.PossibleSchedule + " from: " + proposal.ResourceAgent + "!");


            // if all Machines Answered
            if (workItem.Proposals.Count == _machineAgents.Count)
            {
                
                // item Postponed by All Machines ? -> reque after given amount of time.
                if (workItem.Proposals.TrueForAll(x => x.Postponed))
                {
                    // Call Hub Agent to Requeue
                    workItem = workItem.UpdateResourceAgent(ActorRefs.NoSender)
                                       .UpdateStatus(ElementStatus.Created);
                    _workItemQueue.Replace(workItem);
                    CreateAndEnqueue(Instruction.EnqueueWorkItem.Create(workItem, Self), proposal.PostponedFor);
                    return;
                } else // updateStatus
                {
                    workItem = workItem.UpdateStatus(workItem.WasSetReady ? ElementStatus.Ready : ElementStatus.InQueue);
                }

                // aknowledge Machine -> therefore get Machine -> send aknowledgement
                var acknowledgement = workItem.Proposals.First(x => x.PossibleSchedule == workItem.Proposals.Where(y => y.Postponed == false)
                                                                                                            .Min(p => p.PossibleSchedule)
                                                                 && x.Postponed == false);




                // set Proposal Start for Machine to Reque if time slot is closed.
                workItem = workItem.UpdateEstimations(acknowledgement.PossibleSchedule, acknowledgement.ResourceAgent);
                _workItemQueue.Replace(workItem);
                CreateAndEnqueue(ResourceAgent.Instruction.AcknowledgeProposal.Create(workItem, acknowledgement.ResourceAgent));
            }
        }

        private void AddMachineToHub(Instruction.AddMachineToHub instruction)
        {
            var machine = instruction.GetObjectFromMessage;
            if (machine == null)
            {
                throw new InvalidCastException("Could not Cast MachineAgent on InstructionSet.ObjectToProcess - From:" + Sender.Path.Name);
            }
            // Add Machine to Pool
            _machineAgents.Add(machine.Ref);
            // Added Machine Agent To Machine Pool
            DebugMessage("Added Machine Agent " + machine.Ref.Path.Name + " to Machine Pool: " + machine.RequiredFor);

        }

        private void EnqueueWorkItem(WorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");
            }

            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (_workItemQueue.Find(x => x.Key == workItem.Key) == null)
            {
                // Set Comunication agent.
                workItem = workItem.UpdateHubAgent(Self);
                // add TO queue
                _workItemQueue.Add(workItem);
                DebugMessage("Got Item to Enqueue: " + workItem.WorkSchedule.Name + " | with status:" + workItem.Status);
            }
            else
            {
                // reset Item.
                DebugMessage("Got Item to Requeue: " + workItem.WorkSchedule.Name + " | with status:" + workItem.Status);
                workItem.Proposals.Clear();
            }

            foreach (var agent in _machineAgents)
            {
                CreateAndEnqueue(instruction: ResourceAgent.Instruction.RequestProposal.Create(workItem, agent));
            }
        }
    }
}
