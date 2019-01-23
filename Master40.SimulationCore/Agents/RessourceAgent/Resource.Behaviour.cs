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
using static Master40.SimulationCore.Agents.Resource.Properties;
using static Master40.SimulationCore.Agents.Resource;

namespace Master40.SimulationCore.Agents
{
    

    public class ResourceBehaviour : Behaviour
    {
        public ResourceBehaviour(Dictionary<string, object> properties) : base(null, properties) { }
        public static ResourceBehaviour Get()
        {
            
            var properties = new Dictionary<string, object>();

            var processingQueueSize = 1;
            properties.Add(QUEUE_LENGTH, 45); // plaing forecast
            properties.Add(PROGRESS_QUEUE_SIZE, processingQueueSize); // TODO COULD MOVE TO MODEL for CONFIGURATION, May not required anymore
            properties.Add(QUEUE, new List<WorkItem>());
            properties.Add(PROCESSING_QUEUE, new LimitedQueue<WorkItem>(processingQueueSize));
            properties.Add(ITEMS_IN_PROGRESS, false);

            return new ResourceBehaviour(properties);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                //case BasicInstruction.Initialize i: RegisterService(); break;
                case Instruction.SetHubAgent msg: SetHubAgent((Resource)agent, msg.GetObjectFromMessage.Ref); break;
                case Instruction.RequestProposal msg: RequestProposal((Resource)agent, msg.GetObjectFromMessage); break;
                case Instruction.AcknowledgeProposal msg: AcknowledgeProposal((Resource)agent, msg.GetObjectFromMessage); break;
                case Instruction.StartWorkWith msg: StartWorkWith((Resource)agent, msg.GetObjectFromMessage); break;
                case Instruction.DoWork msg: ((Resource)agent).DoWork(); break;
                case Instruction.FinishWork msg: FinishWork((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        public void StartWorkWith(Resource agent, ItemStatus workItemStatus)
        {
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
                // upate Processing queue
                agent.UpdateProcessingQueue(workItem);

                // there is at least Something Ready so Start Work
                agent.DoWork();
            }
        }

        private void RequestProposal(Resource agent, WorkItem workItem)
        {
            if (workItem == null)
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");

            // debug
            agent.DebugMessage("Request for Proposal");
            // Send

            agent.SendProposalTo(workItem);
        }

        public void AcknowledgeProposal(Resource agent, WorkItem workItem)
        {
            if (workItem == null)
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");

            var queue = agent.Get<List<WorkItem>>(QUEUE);
            if (queue.Any(e => e.Priority(agent.CurrentTime) <= workItem.Priority(agent.CurrentTime)))
            {
                // Get item Latest End.
                var maxItem = queue.Where(e => e.Priority(agent.CurrentTime) <= workItem.Priority(agent.CurrentTime)).Max(e => e.EstimatedEnd);

                // check if Queuable
                if (maxItem > workItem.EstimatedStart)
                {
                    // reset Agent Status
                    workItem = workItem.UpdateStatus(ElementStatus.Created);
                    agent.SendProposalTo(workItem);
                    return;
                }
            }

            agent.DebugMessage("AcknowledgeProposal and Enqueued Item: " + workItem.WorkSchedule.Name);
            queue.Add(workItem);

            // Enqued before another item?
            var position = queue.OrderBy(x => x.Priority(agent.CurrentTime)).ToList().IndexOf(workItem);
            agent.DebugMessage("Position: " + position + " Priority:" + workItem.Priority(agent.CurrentTime) + " Queue length " + queue.Count());

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < queue.Count)
            {
                var toRequeue = queue.OrderBy(x => x.Priority(agent.CurrentTime)).ToList().GetRange(position + 1, queue.Count() - position - 1);

                agent.CallToReQueue(toRequeue);

                agent.DebugMessage("New Queue length = " + queue.Count);
            }


            if (workItem.Status == ElementStatus.Ready)
            {
                // update Processing queue
                agent.UpdateProcessingQueue(workItem);

                // there is at least Something Ready so Start Work
                agent.DoWork();
            }
        }

        private void FinishWork(Resource agent, WorkItem workItem)
        {
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
            agent.UpdateProcessingQueue(itemFromQueue);

            // Set Machine State to Ready for next
            itemsInProgress = false;
            agent.DebugMessage("Finished Work with " + workItem.WorkSchedule.Name + " take next...");

            workItem = workItem.UpdateStatus(ElementStatus.Finished);

            // Call Hub Agent that item has ben processed.
            agent.Send(Hub.Instruction.FinishWorkItem.Create(workItem, workItem.HubAgent));


            // Reorganize List
            agent.CallToReQueue(Queue.Where(x => x.Status == ElementStatus.Created || x.Status == ElementStatus.InQueue).ToList());
            // do Do Work in next Timestep.
            agent.Send(Instruction.DoWork.Create(null, agent.Context.Self));
        }

        private void SetHubAgent(Resource agent, IActorRef hubAgent)
        {
            // save Cast to expected object
            var _hub = hubAgent as IActorRef;

            // throw if cast failed.
            if (_hub == null)
                throw new ArgumentException("Could not Cast Communication ActorRef from Instruction");

            // Save to Value Store
            agent.ValueStore.Add(HUB_AGENT_REF, hubAgent);
            // Debug Message
            agent.DebugMessage("Successfull Registred Service at : " + _hub.Path.Name);
        }
    }
}
