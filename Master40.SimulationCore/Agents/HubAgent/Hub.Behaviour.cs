using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Agents.Ressource;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.HubAgent
{
    public class HubBehaviour : Behaviour
    {
        private HubBehaviour(Dictionary<string, object> properties) : base(null, properties) { }

        public static HubBehaviour Get(string skillGroup)
        {
            var properties = new Dictionary<string, object>();

            properties.Add(Hub.Properties.WORK_ITEM_QUEUE, new List<FWorkItem>());
            properties.Add(Hub.Properties.MACHINE_AGENTS, new Dictionary<IActorRef, string>());
            properties.Add(Hub.Properties.SKILL_GROUP, skillGroup);

            return new HubBehaviour(properties);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Hub.Instruction.EnqueueWorkItem msg: EnqueueWorkItem((Hub)agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.ProductionStarted msg: ProductionStarted((Hub)agent, msg.GetObjectfromMessage); break;
                case Hub.Instruction.FinishWorkItem msg: FinishWorkItem((Hub)agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.ProposalFromMachine msg: ProposalFromMachine((Hub)agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.SetWorkItemStatus msg: SetWorkItemStatus((Hub)agent, msg.GetObjectFromMessage); break;
                case Hub.Instruction.AddMachineToHub msg: AddMachineToHub((Hub)agent, msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown((Hub)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ResourceBreakDown(Hub agent, FBreakDown breakDown)
        {
            var machineAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.MACHINE_AGENTS);
            var brockenMachine = machineAgents.Single(x => breakDown.Machine == x.Value).Key;
            machineAgents.Remove(brockenMachine);
            agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakDown, brockenMachine, true), 45);

            System.Diagnostics.Debug.WriteLine("Break for " + breakDown.Machine, "Hub");
        }

        private void EnqueueWorkItem(Hub agent, FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");
            }


            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);
            var machineAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.MACHINE_AGENTS);
            var localItem = workItemQueue.Find(x => x.Key == workItem.Key);
            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (localItem == null)
            {
                // Set Comunication agent.
                localItem = workItem.UpdateHubAgent(agent.Context.Self);
                // add TO queue
                workItemQueue.Add(localItem);
                agent.DebugMessage("Got Item to Enqueue: " + workItem.Operation.Name + " | with status:" + workItem.Status);
            }
            else
            {
                // reset Item.
                agent.DebugMessage("Got Item to Requeue: " + workItem.Operation.Name + " | with status:" + workItem.Status);
                localItem.Proposals.Clear();
            }

            foreach (var actorRef in machineAgents)
            {
                agent.Send(instruction: Resource.Instruction.RequestProposal.Create(localItem, actorRef.Key));
            }
        }

        public void ProductionStarted(Hub agent, FWorkItem workItem)
        {
            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);
            var temp = workItemQueue.Single(x => x.Key == workItem.Key);
            var tempItem = workItem.UpdatePoductionAgent(temp.ProductionAgent);
            agent.Send(Production.Instruction
                                      .ProductionStarted
                                      .Create(message: tempItem
                                             , target: temp.ProductionAgent));
            workItemQueue.Replace(tempItem);
        }

        public void FinishWorkItem(Hub agent, FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);

            agent.DebugMessage("Machine called " + workItem.Operation.Name + " finished.");
            agent.Send(Production.Instruction.FinishWorkItem.Create(workItem, workItem.ProductionAgent));
            workItemQueue.Remove(workItemQueue.Find(x => x.Key == workItem.Key));
        }

        public void SetWorkItemStatus(Hub agent, FItemStatus workItemStatus)
        {
            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);

            if (workItemStatus == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            // update Status
            var workItem = workItemQueue.Find(x => x.Key == workItemStatus.ItemId)
                                         .UpdateStatus(workItemStatus.Status);

            agent.DebugMessage("Set Item: " + workItem.Operation.Name + " | Status to: " + workItem.Status);
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
                    agent.Send(Resource.Instruction.StartWorkWith.Create(workItemStatus, workItem.ResourceAgent));
                    agent.DebugMessage("Call for Work");
                }
            }
            workItemQueue.Replace(workItem);
        }

        private void ProposalFromMachine(Hub agent, FProposal proposal)
        {
            var workItemQueue = agent.Get<List<FWorkItem>>(Hub.Properties.WORK_ITEM_QUEUE);
            var machineAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.MACHINE_AGENTS);

            if (proposal == null)
            {
                throw new InvalidCastException("Could not Cast Proposal on InstructionSet.ObjectToProcess");
            }
            // get releated workitem and add Proposal.
            var workItem = workItemQueue.Find(x => x.Key == proposal.WorkItemId);
            var proposalToRemove = workItem.Proposals.Find(x => x.ResourceAgent == proposal.ResourceAgent);
            if (proposalToRemove != null)
            {
                workItem.Proposals.Remove(proposalToRemove);
            }

            workItem.Proposals.Add(proposal);

            agent.DebugMessage("Proposal for Schedule: " + proposal.PossibleSchedule + " from: " + proposal.ResourceAgent + "!");


            // if all Machines Answered
            if (workItem.Proposals.Count == machineAgents.Count)
            {

                // item Postponed by All Machines ? -> reque after given amount of time.
                if (workItem.Proposals.TrueForAll(x => x.Postponed))
                {
                    // Call Hub Agent to Requeue
                    workItem = workItem.UpdateResourceAgent(ActorRefs.NoSender)
                                       .UpdateStatus(ElementStatus.Created);
                    workItemQueue.Replace(workItem);
                    agent.Send(Hub.Instruction.EnqueueWorkItem.Create(workItem, agent.Context.Self), proposal.PostponedFor);
                    return;
                }
                else // updateStatus
                {
                    
                    workItem = workItem.UpdateStatus(workItem.WasSetReady? ElementStatus.Ready : ElementStatus.InQueue);
                }

                // aknowledge Machine -> therefore get Machine -> send aknowledgement
                var acknowledgement = workItem.Proposals.First(x => x.PossibleSchedule == workItem.Proposals.Where(y => y.Postponed == false)
                                                                                                            .Min(p => p.PossibleSchedule)
                                                                 && x.Postponed == false);

                // set Proposal Start for Machine to Reque if time slot is closed.
                workItem = workItem.UpdateEstimations(acknowledgement.PossibleSchedule, acknowledgement.ResourceAgent);
                workItemQueue.Replace(workItem);
                agent.Send(Resource.Instruction.AcknowledgeProposal.Create(workItem, acknowledgement.ResourceAgent));
            }
        }

        
        private void AddMachineToHub(Hub agent, FHubInformation hubInformation)
        {
            var machineAgents = agent.Get<Dictionary<IActorRef, string>>(Hub.Properties.MACHINE_AGENTS);
            if (hubInformation == null)
            {
                throw new InvalidCastException("Could not Cast MachineAgent on InstructionSet.ObjectToProcess - From:" + agent.Sender.Path.Name);
            }
            // Add Machine to Pool
            machineAgents.Add(hubInformation.Ref, hubInformation.RequiredFor);
            // Added Machine Agent To Machine Pool
            agent.DebugMessage("Added Machine Agent " + hubInformation.Ref.Path.Name + " to Machine Pool: " + hubInformation.RequiredFor);
        }
    }
}
