﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.Ressource
{


    public class ResourceBehaviour : Behaviour
    {
        public ResourceBehaviour(Dictionary<string, object> properties) : base(null, properties) { }
        public static ResourceBehaviour Get()
        {
            
            var properties = new Dictionary<string, object>();

            var processingQueueSize = 1;
            properties.Add(Resource.Properties.QUEUE_LENGTH, 45); // plaing forecast
            properties.Add(Resource.Properties.PROGRESS_QUEUE_SIZE, processingQueueSize); // TODO COULD MOVE TO MODEL for CONFIGURATION, May not required anymore
            properties.Add(Resource.Properties.QUEUE, new List<FWorkItem>());
            properties.Add(Resource.Properties.PROCESSING_QUEUE, new LimitedQueue<FWorkItem>(processingQueueSize));
            properties.Add(Resource.Properties.ITEMS_IN_PROGRESS, false);

            return new ResourceBehaviour(properties);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                //case BasicInstruction.Initialize i: RegisterService(); break;
                case Resource.Instruction.SetHubAgent msg: SetHubAgent((Resource)agent, msg.GetObjectFromMessage.Ref); break;
                case Resource.Instruction.RequestProposal msg: RequestProposal((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.AcknowledgeProposal msg: AcknowledgeProposal((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.StartWorkWith msg: StartWorkWith((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.DoWork msg: ((Resource)agent).DoWork(); break;
                case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.FinishWork msg: FinishWork((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        public void StartWorkWith(Resource agent, FItemStatus workItemStatus)
        {
            if (workItemStatus == null)
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            
            var Queue = agent.Get<List<FWorkItem>>(Resource.Properties.QUEUE);
            // update Status
            var workItem = Queue.FirstOrDefault(x => x.Key == workItemStatus.ItemId);

            if (workItem != null && workItemStatus.Status == ElementStatus.Ready)
            {
                Queue.Replace(workItem.UpdateStatus(workItem.Status));
                agent.DebugMessage("Set Item: " + workItem.Operation.Name + " | Status to: " + workItem.Status + " with Id: " + workItem.Key);
                // upate Processing queue
                agent.UpdateProcessingQueue(workItem);

                // there is at least Something Ready so Start Work
                agent.DoWork();
            }
        }

        /// <summary>
        /// Is Called from Comunication Agent to get an Proposal when the item with a given priority can be scheduled.
        /// </summary>
        /// <param name="instructionSet"></param>
        private void RequestProposal(Resource agent, FWorkItem workItem)
        {
            if (workItem == null)
                throw new InvalidCastException("Could not Cast Workitem on InstructionSet.ObjectToProcess");

            // debug
            agent.DebugMessage("Request for Proposal: " + workItem.Operation.Name + " with Id: " + workItem.Key + ")");
            // Send

            agent.SendProposalTo(workItem);
        }

        /// <summary>
        /// is Called if The Proposal is accepted by Comunication Agent
        /// </summary>
        /// <param name="instructionSet"></param>
        public void AcknowledgeProposal(Resource agent, FWorkItem workItem)
        {

            agent.DebugMessage("AcknowledgeProposal Item: " + workItem.Operation.Name + " WorkItemId: " + workItem.Key);
            var queue = agent.Get<List<FWorkItem>>(Resource.Properties.QUEUE);
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

            agent.DebugMessage("AcknowledgeProposal Accepted Item: " + workItem.Operation.Name + " with Id: " + workItem.Key);
            workItem = workItem.UpdateStatus(ElementStatus.InQueue);
            queue.Add(workItem);

            // Enqued before another item?
            var position = queue.OrderBy(x => x.Priority(agent.CurrentTime)).ToList().IndexOf(workItem);
            agent.DebugMessage("Position: " + position + " Priority:" + workItem.Priority(agent.CurrentTime) + " Queue length " + queue.Count());

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < queue.Count)
            {
                var toRequeue = queue.OrderBy(x => x.Priority(agent.CurrentTime)).ToList().GetRange(position + 1, queue.Count() - position - 1);

                agent.CallToReQueue(queue, toRequeue);

                agent.DebugMessage("New Queue length = " + queue.Count);
            }


            if (workItem.Status == ElementStatus.InQueue && workItem.MaterialsProvided == true)
            {
                // update Processing queue
                agent.UpdateProcessingQueue(workItem);

                // there is at least Something Ready so Start Work
                agent.DoWork();
            }
        }

        private void FinishWork(Resource agent, FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            // Call Hub Agent that item has ben processed.
            workItem = workItem.UpdateStatus(ElementStatus.Finished);
            agent.DebugMessage("Finished Work with " + workItem.Operation.Name + " with Id: " + workItem.Key + " take next...");
            agent.Send(Hub.Instruction.FinishWorkItem.Create(workItem, workItem.HubAgent));
            // Set Machine State to Ready for next
            agent.Set(Resource.Properties.ITEMS_IN_PROGRESS, false);

            // Set next Ready Element from Queue
            var Queue = agent.Get<List<FWorkItem>>(Resource.Properties.QUEUE);
            var itemFromQueue = Queue.Where(x => x.Status == ElementStatus.Ready)
                                     .OrderBy(x => x.Priority(agent.CurrentTime))
                                        .ThenBy(x => x.Operation.Duration)
                                     .FirstOrDefault();

            //get next ready element and put it to processing queue
            if (itemFromQueue != null) { 
                agent.UpdateProcessingQueue(itemFromQueue);
                agent.DebugMessage("After Finish Work with " + workItem.Key + " start working Id " + itemFromQueue.Key + " take next...");
            }

            // Reorganize List
            agent.CallToReQueue(Queue, Queue.Where(x => x.Status == ElementStatus.Created 
                                                     || x.Status == ElementStatus.InQueue
                                                     || x.Status == ElementStatus.Ready).ToList());
            // do Do Work in next Timestep.
            agent.Send(Resource.Instruction.DoWork.Create(null, agent.Context.Self));
        }


        /// <summary>
        /// Register the Machine in the System on Startup and Save the Hub agent.
        /// </summary>
        private void SetHubAgent(Resource agent, IActorRef hubAgent)
        {
            // save Cast to expected object
            var _hub = hubAgent as IActorRef;

            // throw if cast failed.
            if (_hub == null)
                throw new ArgumentException("Could not Cast Communication ActorRef from Instruction");

            // Save to Value Store
            agent.Set(Resource.Properties.HUB_AGENT_REF, hubAgent);
            // Debug Message
            agent.DebugMessage("Successfull Registred Service at : " + _hub.Path.Name);
        }

        private void BreakDown(Resource agent, FBreakDown breakDwon)
        {
            if (breakDwon.IsBroken)
            {
                Break(agent, breakDwon);
            } else
            {
                RecoverFromBreakDown(agent);
            }

        }

        private void Break(Resource agent, FBreakDown breakdown)
        {
            agent.Set(Resource.Properties.BROKEN, breakdown.IsBroken);
            // requeue all
            var queue = agent.Get<List<FWorkItem>>(Resource.Properties.QUEUE);
            var Processing = agent.Get<LimitedQueue<FWorkItem>>(Resource.Properties.PROCESSING_QUEUE);
            agent.CallToReQueue(Processing, new List<FWorkItem>(Processing));
            agent.CallToReQueue(queue, new List<FWorkItem>(queue));
            // set Self Recovery
            agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakdown.SetIsBroken(false), agent.Context.Self), 1440);
        }

        private void RecoverFromBreakDown(Resource agent)
        {
            agent.Set(Resource.Properties.BROKEN, false);
            agent.Send(Hub.Instruction.AddMachineToHub.Create(new FHubInformation(ResourceType.Machine, agent.Name, agent.Context.Self), agent.VirtualParent, true));
        }

    }
}