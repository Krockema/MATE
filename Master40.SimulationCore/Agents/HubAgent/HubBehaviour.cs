using Akka.Actor;
using AkkaSim.Interfaces;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Master40.SimulationCore.Agents.HubAgent.Instruction;

namespace Master40.SimulationCore.Agents
{
    public static class HubBehaviour
    {
        public const string WORK_ITEM_QUEUE = "WorkItemQueue";
        public const string MACHINE_AGENTS= "MachineAgents";
        public const string SKILL_GROUP = "SkillGroup";

        public static BehaviourSet Default(string skillGroup)
        {
            var actions = new Dictionary<Type, Action<Agent, ISimulationMessage>>();
            var properties = new Dictionary<string, object>();

            properties.Add(WORK_ITEM_QUEUE, new List<WorkItem>());
            properties.Add(MACHINE_AGENTS, new List<IActorRef>());
            properties.Add(SKILL_GROUP, skillGroup);

            actions.Add(typeof(EnqueueWorkItem), EnqueueWorkItem);
            actions.Add(typeof(ProductionStarted), ProductionStarted);
            actions.Add(typeof(FinishWorkItem), FinishWorkItem);
            actions.Add(typeof(SetWorkItemStatus), SetWorkItemStatus);
            actions.Add(typeof(ProposalFromMachine), ProposalFromMachine);
           // actions.Add(typeof(ResponseFromStock), ResponseFromStock);

            return new BehaviourSet(actions);
        }

        public static Action<Agent, ISimulationMessage> EnqueueWorkItem = (agent, item) =>
        {
            var workItem = item.Message as WorkItem;
            var workItemQueue = agent.Get<List<WorkItem>>(WORK_ITEM_QUEUE);
            var machineAgents = agent.Get<List<IActorRef>>(MACHINE_AGENTS);
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");
            }

            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (workItemQueue.Find(x => x.Key == workItem.Key) == null)
            {
                // Set Comunication agent.
                workItem = workItem.UpdateHubAgent(agent.Context.Self);
                // add TO queue
                workItemQueue.Add(workItem);
                agent.DebugMessage("Got Item to Enqueue: " + workItem.WorkSchedule.Name + " | with status:" + workItem.Status);
            }
            else
            {
                // reset Item.
                agent.DebugMessage("Got Item to Requeue: " + workItem.WorkSchedule.Name + " | with status:" + workItem.Status);
                workItem.Proposals.Clear();
            }

            foreach (var actorRef in machineAgents)
            {
                agent.Send(instruction: ResourceAgent.Instruction.RequestProposal.Create(workItem, actorRef));
            }
        };

        public static Action<Agent, ISimulationMessage> ProductionStarted = (agent, item) =>
        {
            var workItem = item.Message as WorkItem;
            var workItemQueue = agent.Get<List<WorkItem>>(WORK_ITEM_QUEUE);
            var temp = workItemQueue.Single(x => x.Key == workItem.Key);
            agent.Send(ProductionAgent.Instruction
                                      .ProductionStarted
                                      .Create(message: workItem.UpdatePoductionAgent(temp.ProductionAgent)
                                             , target: temp.ProductionAgent));

        };

        public static Action<Agent, ISimulationMessage> FinishWorkItem = (agent, item) =>
        {
            var workItem = item.Message as WorkItem;
            var workItemQueue = agent.Get<List<WorkItem>>(WORK_ITEM_QUEUE);


            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            agent.DebugMessage("Machine called " + workItem.WorkSchedule.Name + " finished.");
            agent.Send(ProductionAgent.Instruction.FinishWorkItem.Create(workItem, workItem.ProductionAgent));
            workItemQueue.Remove(workItemQueue.Find(x => x.Key == workItem.Key));
        };

        public static Action<Agent, ISimulationMessage> SetWorkItemStatus = (agent, item) =>
        {
            var workItemStatus = item.Message as ItemStatus;
            var workItemQueue = agent.Get<List<WorkItem>>(WORK_ITEM_QUEUE);

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
                    agent.Send(ResourceAgent.Instruction.StartWorkWith.Create(workItemStatus, workItem.ResourceAgent));
                    agent.DebugMessage("Call for Work");
                }
            }
            workItemQueue.Replace(workItem);
        };

        public static Action<Agent, ISimulationMessage> ProposalFromMachine = (agent, item) =>
        {
            var proposal = item.Message as Proposal;
            var workItemQueue = agent.Get<List<WorkItem>>(WORK_ITEM_QUEUE);
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
                    agent.Send(HubAgent.Instruction.EnqueueWorkItem.Create(workItem, agent.Context.Self), proposal.PostponedFor);
                    return;
                }
                else // updateStatus
                {
                    workItem = workItem.UpdateStatus(workItem.WasSetReady ? ElementStatus.Ready : ElementStatus.InQueue);
                }

                // aknowledge Machine -> therefore get Machine -> send aknowledgement
                var acknowledgement = workItem.Proposals.First(x => x.PossibleSchedule == workItem.Proposals.Where(y => y.Postponed == false)
                                                                                                            .Min(p => p.PossibleSchedule)
                                                                 && x.Postponed == false);

                // set Proposal Start for Machine to Reque if time slot is closed.
                workItem = workItem.UpdateEstimations(acknowledgement.PossibleSchedule, acknowledgement.ResourceAgent);
                workItemQueue.Replace(workItem);
                agent.Send(ResourceAgent.Instruction.AcknowledgeProposal.Create(workItem, acknowledgement.ResourceAgent));
            }
        };
    }
}
