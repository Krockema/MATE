using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        public Default(int planingJobQueueLength, int fixedJobQueueSize, SimulationType simulationType = SimulationType.None) : base(null, simulationType)
        {
            this.processingQueue = new JobQueueItemLimited(fixedJobQueueSize);
            this.queue = new JobQueueTimeLimited(planingJobQueueLength);
            AgentDictionary = new AgentDictionary();
            this.queueLength = queueLength;
        }

        internal int queueLength { get; set; }
        internal JobQueueTimeLimited queue { get; set; }
        internal JobQueueItemLimited processingQueue { get; set; }
        internal bool operationInProgress { get; set; } = false;
        internal WorkTimeGenerator workTimeGenerator { get; }
        internal AgentDictionary AgentDictionary { get; }
        
        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                //case BasicInstruction.Initialize i: RegisterService(); break;
                case Resource.Instruction.SetHubAgent msg: SetHubAgent(agent, msg.GetObjectFromMessage.Ref); break;
                // case Resource.Instruction.RequestProposal msg: RequestProposal((Resource)agent, msg.GetObjectFromMessage); break;
                // case Resource.Instruction.AcknowledgeProposal msg: AcknowledgeProposal((Resource)agent, msg.GetObjectFromMessage); break;
                // case Resource.Instruction.StartWorkWith msg: StartWorkWith((Resource)agent, msg.GetObjectFromMessage); break;
                // case Resource.Instruction.DoWork msg: ((Resource)agent).DoWork(); break;
                // case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                // case Resource.Instruction.FinishWork msg: FinishWork((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /*
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
            if (itemFromQueue != null)
            {
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

        */
        /// <summary>
        /// Register the Machine in the System on Startup and Save the Hub agent.
        /// </summary>
        private void SetHubAgent(Agent agent, IActorRef hubAgent)
        {
            // Save to Value Store
            AgentDictionary.Add(hubAgent, "Default");
            // Debug Message
            agent.DebugMessage("Successfull Registred Service at : " + hubAgent.Path.Name);
        }

        /*
        private void BreakDown(Resource agent, FBreakDown breakDwon)
        {
            if (breakDwon.IsBroken)
            {
                Break(agent, breakDwon);
            }
            else
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

         internal void UpdateProcessingQueue(FWorkItem workItem)
        {
            var processingQueue = Get<LimitedQueue<FWorkItem>>(Properties.PROCESSING_QUEUE);
            var queue = Get<List<FWorkItem>>(Properties.QUEUE);
            var hub = Get<IActorRef>(Properties.HUB_AGENT_REF);
            if (processingQueue.CapacitiesLeft && workItem != null)
            {
                if (workItem.Operation.HierarchyNumber == 10)
                    Send(Hub.Instruction.ProductionStarted.Create(workItem, hub));
                processingQueue.Enqueue(workItem);
                queue.Remove(workItem);
            }
        }

        internal void CallToReQueue(List<FWorkItem> queue, List<FWorkItem> toRequeue)
        {
            foreach (var reqItem in toRequeue)
            {
                DebugMessage("-> ToRequeue " + reqItem.Priority(TimePeriod) + " Current Possition: " + queue.OrderBy(x => x.Priority(TimePeriod)).ToList().IndexOf(reqItem) + " Id " + reqItem.Key);

                // remove item from current Queue
                queue.Remove(reqItem);

                // Call Comunication Agent to Requeue
                Send(Hub.Instruction.EnqueueWorkItem.Create(reqItem.UpdateStatus(ElementStatus.Created), reqItem.HubAgent));
            }
        }

        /// <summary>
        /// Starts the next WorkItem
        /// </summary>
        internal void DoWork()
        {
            if (Get<bool>(Properties.ITEMS_IN_PROGRESS))
            {
                DebugMessage("Im still working....");
                return; // still working
            }

            // Dequeue
            var processingQueue = Get<LimitedQueue<FWorkItem>>(Properties.PROCESSING_QUEUE);
            var item = processingQueue.Dequeue();


            // Wait if nothing More todo.
            if (item == null)
            {
                // No more work 
                DebugMessage("Nothing more Ready in Queue!");
                return;
            }

            DebugMessage("Start with " + item.Operation.Name);
            Set(Properties.ITEMS_IN_PROGRESS, true);
            item = item.UpdateStatus(ElementStatus.Processed);


            // TODO: Roll delay here
            var workTimeGenerator = Get<WorkTimeGenerator>(Properties.WORK_TIME_GENERATOR);
            var duration = workTimeGenerator.GetRandomWorkTime(item.Operation.Duration);

            //Debug.WriteLine("Duration: " + duration + " for " + item.WorkSchedule.Name);

            // TODO !
            var pub = new UpdateSimulationWork(item.Key.ToString(), duration - 1, (int)this.TimePeriod, this.Name);
            this.Context.System.EventStream.Publish(pub);
            // Statistics.UpdateSimulationWorkSchedule(item.Id.ToString(), (int)Context.TimePeriod, duration - 1, this.Machine);

            // get item = ready and lowest priority
            Send(Instruction.FinishWork.Create(item, Context.Self), duration);
        }

        /// <summary>
        /// Send Proposal to Comunication Client
        /// </summary>
        /// <param name="workItem"></param>
        internal void SendProposalTo(FWorkItem workItem)
        {
            long max = 0;
            var queue = this.Get<List<FWorkItem>>(Properties.QUEUE);
            var queueLength = this.Get<int>(Properties.QUEUE_LENGTH);

            if (queue.Any(e => e.Priority(this.CurrentTime) <= workItem.Priority(this.CurrentTime)))
            {
                max = queue.Where(e => e.Priority(this.CurrentTime) <= workItem.Priority(this.CurrentTime)).Max(e => e.EstimatedEnd);
            }

            // calculat Proposal.
            var proposal = new FProposal(possibleSchedule: max
                                            , postponed: (max > queueLength && workItem.Status != ElementStatus.Ready)
                                            , postponedFor: queueLength
                                            , workItemId: workItem.Key
                                            , resourceAgent: this.Context.Self);

            // callback 
            this.Send(Hub.Instruction.ProposalFromMachine.Create(proposal, this.Context.Sender));
        }
        */
    }
}
