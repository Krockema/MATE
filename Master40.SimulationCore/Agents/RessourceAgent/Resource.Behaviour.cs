using System;
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
            properties.Add(Resource.Properties.QUEUE, new List<FBucket>());
            properties.Add(Resource.Properties.PROCESSING_QUEUE, new LimitedQueue<FBucket>(processingQueueSize));
            properties.Add(Resource.Properties.ITEMS_IN_PROGRESS, false);
            properties.Add(Resource.Properties.EQUP_RESOURCETOOL, null);
            return new ResourceBehaviour(properties);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                //case BasicInstruction.Initialize i: RegisterService(); break;
                case Resource.Instruction.SetHubAgent msg: SetHubAgent((Resource)agent, msg.GetObjectFromMessage.Ref); break;
                case Resource.Instruction.RequestProposalBucket msg: RequestProposal((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.AcknowledgeProposalBucket msg: AcknowledgeProposal((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.StartWorkWith msg: StartWorkWith((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.StartWorkWithNextItem msg: StartWorkWithNextWorkItem((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.DoWork msg: ((Resource)agent).DoWork(); break;
                case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.FinishSetup msg: FinishSetup((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.FinishWorkItem msg: FinishWorkItem((Resource)agent, msg.GetObjectFromMessage); break;
                case Resource.Instruction.FinishWorkBucket msg: FinishWorkBucket((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }


        public void StartWorkWith(Resource agent, FItemStatus workItemStatus)
        {
            if (workItemStatus == null)
                throw new InvalidCastException("Could not Cast >BuucketItemStatus< on InstructionSet.ObjectToProcess");
            
            var Queue = agent.Get<List<FBucket>>(Resource.Properties.QUEUE);
            // update Status
            var bucketItem = Queue.FirstOrDefault(x => x.Key == workItemStatus.ItemId);

            if (bucketItem != null && workItemStatus.Status == ElementStatus.Ready)
            {
                Queue.Remove(bucketItem);
                bucketItem = bucketItem.UpdateStatus(workItemStatus.Status);
                Queue.Add(bucketItem);

                agent.DebugMessage("Set Item: " + bucketItem.Key + " | Status to: " + bucketItem.Status);
                // upate Processing queue
                agent.UpdateProcessingQueue(bucketItem);

                // there is at least Something Ready so Start Work
                agent.DoWork();
            }
        }

        private void StartWorkWithNextWorkItem(Resource agent, FBucket bucket)
        {



            throw new NotImplementedException();
        }

        /// <summary>
        /// Is Called from Comunication Agent to get an Proposal when the item with a given priority can be scheduled.
        /// </summary>
        /// <param name="instructionSet"></param>
        private void RequestProposal(Resource agent, FBucket bucket)
        {
            if (bucket.GetType() != typeof(FBucket))
            {
                throw new InvalidCastException("Could not Cast >Bucket< on InstructionSet.ObjectToProcess");
            }
            // debug
            agent.DebugMessage("Request for Proposal");
            
            // Send
            agent.SendProposalTo(bucket);
        }

        /// <summary>
        /// is Called if The Proposal is accepted by Comunication Agent
        /// </summary>
        /// <param name="instructionSet"></param>
        public void AcknowledgeProposal(Resource agent, FBucket bucket)
        {
            var bucketQueue = agent.Get<List<FBucket>>(Resource.Properties.QUEUE);

            if (bucketQueue.Any(e => e.Priority(agent.CurrentTime) <= bucket.Priority(agent.CurrentTime)))
            {
                // Get item Latest End.
                var maxItem = bucketQueue.Where(e => e.Priority(agent.CurrentTime) <= bucket.Priority(agent.CurrentTime)).Max(e => e.EstimatedEnd);

                // check if Queuable
                if (maxItem > bucket.EstimatedStart)
                {
                    // reset Agent Status
                    bucket = bucket.UpdateStatus(ElementStatus.Created);
                    agent.SendProposalTo(bucket);
                    return;
                }
            }

            agent.DebugMessage("AcknowledgeProposal and Enqueued Bucket: " + bucket.Key.ToString());
            bucketQueue.Add(bucket);

            // Enqued before another item?
            var position = bucketQueue.OrderBy(x => x.Priority(agent.CurrentTime)).ToList().IndexOf(bucket);
            agent.DebugMessage("Position: " + position + " Priority:" + bucket.Priority(agent.CurrentTime) + " Queue length " + bucketQueue.Count());

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < bucketQueue.Count)
            {
                var toRequeue = bucketQueue.OrderBy(x => x.Priority(agent.CurrentTime)).ToList().GetRange(position + 1, bucketQueue.Count() - position - 1);

                agent.CallToReQueue(bucketQueue, toRequeue);

                agent.DebugMessage("New Queue length = " + bucketQueue.Count);
            }


            if (bucket.Status == ElementStatus.Ready)
            {
                // update Processing queue
                agent.UpdateProcessingQueue(bucket);

                // there is at least Something Ready so Start Work
                agent.DoWork();
            }
        }

        private void FinishSetup(Resource agent, FBucket bucket)
        {
            if (bucket == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            agent.processWorkItemsFromBucket(bucket);

        }

        private void FinishWorkItem(Resource agent, FWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            workItem = workItem.UpdateStatus(ElementStatus.Finished);

            // Call Hub Agent that item has ben processed.
            agent.Send(Hub.Instruction.FinishWorkItem.Create(workItem, workItem.HubAgent));


          
            // do Do Work in next Timestep.
            agent.Send(Resource.Instruction.DoWork.Create(null, agent.Context.Self));

        }

        private void FinishWorkBucket(Resource agent, FBucket bucket)
        {
            if (bucket == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            var bucketQueue = agent.Get<List<FBucket>>(Resource.Properties.QUEUE);

            // Set next Ready Element from Queue
            FBucket itemFromQueue = bucketQueue.Where(x => x.Status == ElementStatus.Ready)
                                     .OrderBy(x => x.Priority(agent.CurrentTime)).FirstOrDefault();

            agent.UpdateProcessingQueue(itemFromQueue);

            // Reorganize List
            agent.CallToReQueue(bucketQueue, bucketQueue.Where(x => x.Status == ElementStatus.Created || x.Status == ElementStatus.InQueue).ToList());

            // Set Machine State to Ready for next
            agent.Set(Resource.Properties.ITEMS_IN_PROGRESS, false);
            agent.DebugMessage("Finished Work with " + itemFromQueue.Key + " take next...");
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
            var queue = agent.Get<List<FBucket>>(Resource.Properties.QUEUE);
            var Processing = agent.Get<LimitedQueue<FBucket>>(Resource.Properties.PROCESSING_QUEUE);
            agent.CallToReQueue(Processing, new List<FBucket>(Processing));
            agent.CallToReQueue(queue, new List<FBucket>(queue));
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
