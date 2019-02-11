using Akka.Actor;
using AkkaSim.Interfaces;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Master40.SimulationCore.Agents.Hub;
using static Master40.SimulationCore.Agents.Hub.Properties;

namespace Master40.SimulationCore.Agents
{
    public class HubBehaviour : Behaviour
    {
        private HubBehaviour(Dictionary<string, object> properties) : base(null, properties) { }

        public static HubBehaviour Get(string skillGroup)
        {
            var properties = new Dictionary<string, object>();

            properties.Add(WORK_ITEM_QUEUE, new List<FWorkItem>());
            properties.Add(MACHINE_AGENTS, new List<IActorRef>());
            properties.Add(SKILL_GROUP, skillGroup);

            return new HubBehaviour(properties);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Instruction.EnqueueWorkItem msg: EnqueueWorkItem((Hub)agent, msg.GetObjectFromMessage); break;
                case Instruction.ProductionStarted msg: ProductionStarted((Hub)agent, msg.GetObjectfromMessage); break;
                case Instruction.FinishWorkItem msg: FinishWorkItem((Hub)agent, msg.GetObjectFromMessage); break;
                case Instruction.ProposalFromMachine msg: ProposalFromMachine((Hub)agent, msg.GetObjectFromMessage); break;
                case Instruction.SetWorkItemStatus msg: SetWorkItemStatus((Hub)agent, msg.GetObjectFromMessage); break;
                case Instruction.AddMachineToHub msg: AddMachineToHub((Hub)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void EnqueueWorkItem(Hub agent, FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");
            }


            var workItemQueue = agent.Get<List<FWorkItem>>(WORK_ITEM_QUEUE);
            var machineAgents = agent.Get<List<IActorRef>>(MACHINE_AGENTS);
            var localItem = workItemQueue.Find(x => x.Key == workItem.Key);
            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (localItem == null)
            {
                // Set Comunication agent.
                localItem = workItem.UpdateHubAgent(agent.Context.Self);
                // add TO queue
                workItemQueue.Add(localItem);
                agent.DebugMessage("Got Item to Enqueue: " + workItem.WorkSchedule.Name + " | with status:" + workItem.Status);
            }
            else
            {
                // reset Item.
                agent.DebugMessage("Got Item to Requeue: " + workItem.WorkSchedule.Name + " | with status:" + workItem.Status);
                localItem.Proposals.Clear();
            }

            foreach (var actorRef in machineAgents)
            {
                agent.Send(instruction: Resource.Instruction.RequestProposal.Create(localItem, actorRef));
            }
        }

        public void ProductionStarted(Hub agent, FWorkItem workItem)
        {
            var workItemQueue = agent.Get<List<FWorkItem>>(WORK_ITEM_QUEUE);
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
            var workItemQueue = agent.Get<List<FWorkItem>>(WORK_ITEM_QUEUE);


            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            agent.DebugMessage("Machine called " + workItem.WorkSchedule.Name + " finished.");
            agent.Send(Production.Instruction.FinishWorkItem.Create(workItem, workItem.ProductionAgent));
            workItemQueue.Remove(workItemQueue.Find(x => x.Key == workItem.Key));
        }

        public void SetWorkItemStatus(Hub agent, FItemStatus workItemStatus)
        {
            var workItemQueue = agent.Get<List<FWorkItem>>(WORK_ITEM_QUEUE);

            if (workItemStatus == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            // update Status
            var workItem = workItemQueue.Find(x => x.Key == workItemStatus.ItemId)
                                         .UpdateStatus(workItemStatus.Status);

            agent.DebugMessage("Set Item: " + workItem.WorkSchedule.Name + " | Status to: " + workItem.Status);
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
            var workItemQueue = agent.Get<List<FWorkItem>>(WORK_ITEM_QUEUE);
            var machineAgents = agent.Get<List<IActorRef>>(MACHINE_AGENTS);

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
                    agent.Send(Instruction.EnqueueWorkItem.Create(workItem, agent.Context.Self), proposal.PostponedFor);
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
            var machineAgents = agent.Get<List<IActorRef>>(MACHINE_AGENTS);
            if (hubInformation == null)
            {
                throw new InvalidCastException("Could not Cast MachineAgent on InstructionSet.ObjectToProcess - From:" + agent.Sender.Path.Name);
            }
            // Add Machine to Pool
            machineAgents.Add(hubInformation.Ref);
            // Added Machine Agent To Machine Pool
            agent.DebugMessage("Added Machine Agent " + hubInformation.Ref.Path.Name + " to Machine Pool: " + hubInformation.RequiredFor);

        }
    }
}
