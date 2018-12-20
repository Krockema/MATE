using Akka.Actor;
using AkkaSim.Interfaces;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    public static class ResourceBehaviour
    {
        public const string MACHINE = "Machine"; 
        public const string PROGRESS_QUEUE_SIZE = "ProgressQueueSize"; // int
        public const string QUEUE = "Queue"; // List<WorkItem>
        public const string PROCESSING_QUEUE = "ProcessingQueue"; // LimitedQueue<WorkItem>
        public const string QUEUE_LENGTH = "QueueLength"; // int
        public const string ITEMS_IN_PROGRESS = "ItemInProgress"; // bool
        public const string WORK_TIME_GENERATOR = "WorkTimeGenerator"; // WorkTimeGenerator
        public const string HUB_AGENT_REF = "HubAgentRef"; // IActorRef


        public static BehaviourSet Default()
        {
            var actions = new Dictionary<Type, Action<Agent, ISimulationMessage>>();
            var properties = new Dictionary<string, object>();

            var processingQueueSize = 1;
            properties.Add(QUEUE_LENGTH, 45); // plaing forecast
            properties.Add(PROGRESS_QUEUE_SIZE, processingQueueSize); // TODO COULD MOVE TO MODEL for CONFIGURATION, May not required anymore
            properties.Add(QUEUE, new List<WorkItem>());
            properties.Add(PROCESSING_QUEUE, new LimitedQueue<WorkItem>(processingQueueSize));
            properties.Add(ITEMS_IN_PROGRESS, false);

            actions.Add(typeof(ResourceAgent.Instruction.StartWorkWith), StartWorkWith);
            actions.add(typeof(ResourceAgent.Instruction.RequestProposal), RequestProposal);
            actions.add(typeof(ResourceAgent.Instruction.AcknowledgeProposal), AcknowledgeProposal); 
            actions.add(typeof(),StartWorkWith
            actions.add(typeof(),DoWork instruction: DoWork(); brea
            actions.add(typeof(),FinishWork instruction: FinishWork

            // actions.Add(typeof(RequestArticle), RequestArticle);
            // actions.Add(typeof(ResponseFromStock), ResponseFromStock);

            return new BehaviourSet(actions);
        }

        public static Action<Agent, ISimulationMessage> StartWorkWith = (agent, item) =>
        {
            var workItemStatus = item.Message as ItemStatus;
            var Queue = agent.Get<List<WorkItem>>(QUEUE);
            if (workItemStatus == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            // update Status
            var workItem = Queue.FirstOrDefault(x => x.Key == workItemStatus.ItemId);

            if (workItem != null && workItemStatus.Status == ElementStatus.Ready)
            {
                Queue.Remove(workItem);
                workItem = workItem.UpdateStatus(workItemStatus.Status);
                Queue.Add(workItem);

                agent.DebugMessage("Set Item: " + workItem.WorkSchedule.Name + " | Status to: " + workItem.Status);
                // update Processing queue
                ((ResourceAgent)agent).UpdateProcessingQueue(workItem);

                // there is at least Something Ready so Start Work
                ((ResourceAgent)agent).DoWork();
            }
        };

        public static Action<Agent, ISimulationMessage> FinishWork = (agent, item) =>
        {
            var workItem = item.Message as WorkItem;
            var itemsInProgress = agent.Get<bool>(ITEMS_IN_PROGRESS);
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            var Queue = agent.Get<List<WorkItem>>(QUEUE);

            // Set next Ready Element from Queue
            var itemFromQueue = Queue.Where(x => x.Status == ElementStatus.Ready)
                                     .OrderBy(x => x.Priority(agent.CurrentTime))
                                        .ThenBy(x => x.WorkSchedule.Duration)
                                     .FirstOrDefault();
            ((ResourceAgent)agent).UpdateProcessingQueue(itemFromQueue);

            // Set Machine State to Ready for next
            itemsInProgress = false;
            agent.DebugMessage("Finished Work with " + workItem.WorkSchedule.Name + " take next...");

            workItem = workItem.UpdateStatus(ElementStatus.Finished);

            // Call Hub Agent that item has ben processed.
            agent.Send(HubAgent.Instruction.FinishWorkItem.Create(workItem, workItem.HubAgent));


            // Reorganize List
            ((ResourceAgent)agent).CallToReQueue(Queue.Where(x => x.Status == ElementStatus.Created || x.Status == ElementStatus.InQueue).ToList());
            // do Do Work in next Timestep.
            agent.Send(ResourceAgent.Instruction.DoWork.Create(null, agent.Context.Self));
        };

    }
}
