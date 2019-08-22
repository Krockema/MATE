using System;
using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Types;
using static FPostponeds;
using static FProposals;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        public Default(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, SimulationType simulationType = SimulationType.None) : base(childMaker: null, obj: simulationType)
        {
            this._processingQueue = new JobQueueItemLimited(limit: fixedJobQueueSize);
            this._planingQueue = new JobQueueTimeLimited(limit: planingJobQueueLength);
            this._agentDictionary = new AgentDictionary();
            _workTimeGenerator = workTimeGenerator;
        }

        internal JobQueueTimeLimited _planingQueue { get; set; }
        internal JobQueueItemLimited _processingQueue { get; set; }
        internal bool _operationInProgress { get; set; } = false;
        internal WorkTimeGenerator _workTimeGenerator { get; }
        internal AgentDictionary _agentDictionary { get; }
        
        public override bool Action(object message)
        {
            switch (message)
            {
                case Resource.Instruction.SetHubAgent msg: SetHubAgent(hubAgent: msg.GetObjectFromMessage.Ref); break;
                case Resource.Instruction.RequestProposal msg: RequestProposal(msg.GetObjectFromMessage); break;
                case Resource.Instruction.AcknowledgeProposal msg: AcknowledgeProposal(msg.GetObjectFromMessage); break;
                case Resource.Instruction.UpdateArticleProvided msg: UpdateArticleProvided(operationKey: msg.GetObjectFromMessage); break;
                // case Resource.Instruction.StartWorkWith msg: StartWorkWith((Resource)agent, msg.GetObjectFromMessage); break;
                // case Resource.Instruction.DoWork msg: ((Resource)agent).DoWork(); break;
                // case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                // case Resource.Instruction.FinishWork msg: FinishWork((Resource)agent, msg.GetObjectFromMessage); break;

                //for Testing!!!!!!!!!
                default: return true;
            }
            return true;
        }

        /// <summary>
        /// Register the Machine in the System on Startup and Save the Hub agent.
        /// </summary>
        private void SetHubAgent(IActorRef hubAgent)
        {
            // Save to Value Store
            _agentDictionary.Add(key: hubAgent, value: "Default");
            // Debug Message
            Agent.DebugMessage(msg: "Successfully registered resource at : " + hubAgent.Path.Name);
        }

        /// <summary>
        /// Is Called from Comunication Agent to get an Proposal when the item with a given priority can be scheduled.
        /// </summary>
        /// <param name="instructionSet"></param>
        private void RequestProposal(IJob jobItem)
        {
            Agent.DebugMessage($"Request for Proposal: " + jobItem.Name + " with Id: " + jobItem.Key + ")");

            SendProposalTo(jobItem: jobItem);
        }

        /// <summary>
        /// Send Proposal to Comunication Client
        /// </summary>
        /// <param name="jobItem"></param>
        internal void SendProposalTo(IJob jobItem)
        {
            var possibleStartTime = _planingQueue.GetQueueAbleTime(job: jobItem, currentTime: Agent.CurrentTime);
            
            // calculate proposal
            var proposal = new FProposal(possibleSchedule: possibleStartTime
                , postponed: new FPostponed(offset: (possibleStartTime > _planingQueue.Limit && !jobItem.StartConditions.Satisfied)? possibleStartTime : 0)
                , resourceAgent: Agent.Context.Self
                , jobKey: jobItem.Key);

            Agent.Send(Hub.Instruction.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }

        /// <summary>
        /// Is called after RequestProposal if the proposal is accepted by HubAgent
        /// </summary>
        public void AcknowledgeProposal(IJob jobItem)
        {
            Agent.DebugMessage($"Start Acknowledge proposal for: {jobItem.Name} {jobItem.Key}");

            // if not QueueAble
            if (!_planingQueue.IsQueueAble(item: jobItem))
            {
                Agent.Send(Hub.Instruction.EnqueueJob.Create(message: jobItem, target: jobItem.HubAgent));
                return;
            }

            var startTime = _planingQueue.GetQueueAbleTime(jobItem, currentTime: Agent.CurrentTime);
            jobItem = jobItem.UpdateEstimations(startTime, Agent.Context.Self);
            if (!_planingQueue.Enqueue(item: jobItem))
            {
                throw new Exception("Queueing went wrong!");
            };

            Agent.DebugMessage("AcknowledgeProposal Accepted Item: " + jobItem.Name + " with Id: " + jobItem.Key);
            UpdateAndRequeuePlanedJobs(jobItem: jobItem);
            UpdateProcessingQueue();
            //DoWork();
        }

        private void UpdateAndRequeuePlanedJobs(IJob jobItem)
        {
            Agent.DebugMessage("Old planning queue length = " + _planingQueue.Count);
            var toRequeue = _planingQueue.CutTail(currentTime: Agent.CurrentTime, job: jobItem);
            foreach (var job in toRequeue)
            {
                _planingQueue.RemoveJob(job: job);
                Agent.Send(Hub.Instruction.EnqueueJob.Create(message: job, target: job.HubAgent));
            }
            Agent.DebugMessage("New planning queue length = " + _planingQueue.Count);
        }


        private void UpdateProcessingQueue()
        {
            while (_processingQueue.CapacitiesLeft() && _planingQueue.HasQueueAbleJobs())
            {
                var job = _planingQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);
                var ok = _processingQueue.Enqueue(item: job);
                if (!ok)
                {
                    throw new Exception("Something wen wrong with Queueing!");
                }
                //TODO Withdraw at ProcessingQueue or DoWork?
                Agent.DebugMessage($"Start withdraw for article {job.Name} {job.Key}");
                Agent.Send(BasicInstruction.WithdrawRequiredArticles.Create(message: job.Key, target: job.HubAgent));
            }

            Agent.DebugMessage("Jobs ready to start: " + _processingQueue.Count);
            
        }

        private void UpdateArticleProvided(Guid operationKey)
        {
            
            if (_planingQueue.SetOperationArticleProvided(operationKey))
            {
                Agent.DebugMessage(msg: $"UpdateArticleProvided {operationKey}");
                UpdateProcessingQueue();
            }
            //TODO: Do Work
            
        }

        /*
        /// <summary>
        /// Starts the next Job
        /// </summary>
        internal void DoWork()
        {
            if (_operationInProgress)
            {
                Agent.DebugMessage("Im still working....");
                return; // Resource Agent is still working
            }

            var nextOperationToWork = _processingQueue.DequeueFirstSatisfied(Agent.CurrentTime);

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
        
        */
    }
}
