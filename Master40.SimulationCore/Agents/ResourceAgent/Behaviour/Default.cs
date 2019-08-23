using System;
using System.Linq;
using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Types;
using static FOperationResults;
using static FOperations;
using static FPostponeds;
using static FProposals;
using static FUpdateSimulationWorks;
using static FUpdateStartConditions;
using static IJobResults;
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
        // TODO Implement a JobManager
        internal JobQueueTimeLimited _planingQueue { get; set; }
        internal JobQueueItemLimited _processingQueue { get; set; }
        internal JobInProgress _jobInProgress { get; set; } = new JobInProgress();
        internal WorkTimeGenerator _workTimeGenerator { get; }
        internal AgentDictionary _agentDictionary { get; }
        
        public override bool Action(object message)
        {
            switch (message)
            {
                case Resource.Instruction.SetHubAgent msg: SetHubAgent(hubAgent: msg.GetObjectFromMessage.Ref); break;
                case Resource.Instruction.RequestProposal msg: RequestProposal(msg.GetObjectFromMessage); break;
                case Resource.Instruction.AcknowledgeProposal msg: AcknowledgeProposal(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateStartCondition(startCondition: msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                // case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Register the Resource in the System on Startup and Save the Hub agent.
        /// </summary>
        private void SetHubAgent(IActorRef hubAgent)
        {
            // Save to Value Store
            _agentDictionary.Add(key: hubAgent, value: "Default");
            // Debug Message
            Agent.DebugMessage(msg: "Successfully registered resource at : " + hubAgent.Path.Name);
        }

        /// <summary>
        /// Is Called from Hub Agent to get an Proposal when the item with a given priority can be scheduled.
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
            //TODO  DoWork(); ?
        }

        private void RequeueAllRemainingJobs()
        {
            Agent.DebugMessage("Start to Requeue all remaining Jobs");
            var item = _planingQueue.jobs.FirstOrDefault();
            if (item != null)
            {
                UpdateAndRequeuePlanedJobs(jobItem: item);
            }
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

            Agent.DebugMessage($"Jobs ready to start: {_processingQueue.Count} Try to start processing");
        }

        private void UpdateStartCondition(FUpdateStartCondition startCondition)
        {
            Agent.DebugMessage(msg: $"UpdateArticleProvided for article: {startCondition.OperationKey} ArticleProvided: {startCondition.ArticlesProvided} && PreCondition {startCondition.PreCondition}");

            if (_planingQueue.UpdatePreCondition(startCondition: startCondition))
            {
                UpdateProcessingQueue();
                DoWork();
            }
            
        }

        /// <summary>
        /// Starts the next Job
        /// </summary>
        internal void DoWork()
        {
            if (_jobInProgress.IsSet)
            {
                Agent.DebugMessage("Im still working....");
                return; // Resource Agent is still working
            }

            var nextJobInProgress = _processingQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);
            // Wait if nothing more to do
            if (nextJobInProgress == null)
            {
                // No more work 
                Agent.DebugMessage("Nothing more Ready in Queue!");
                return;
            }

            UpdateProcessingQueue();

            _jobInProgress.Set(nextJobInProgress);

            var randomizedDuration = _workTimeGenerator.GetRandomWorkTime(duration: nextJobInProgress.Duration);
            Agent.DebugMessage($"Starting Job {nextJobInProgress.Name}  Key: {nextJobInProgress.Key} new Duration is {randomizedDuration}");

            var pub = new FUpdateSimulationWork(nextJobInProgress.Key.ToString(), randomizedDuration, start: Agent.CurrentTime, machine: Agent.Name);
            Agent.Context.System.EventStream.Publish(@event: pub);

            var fOperationResult = new FOperationResult(key: nextJobInProgress.Key
                                             , creationTime: 0
                                                    , start: Agent.CurrentTime
                                                      , end: Agent.CurrentTime + randomizedDuration
                                         , originalDuration: nextJobInProgress.Duration
                                          , productionAgent: ActorRefs.Nobody
                                            , resourceAgent: Agent.Context.Self);

            Agent.Send(BasicInstruction.FinishJob.Create(message: fOperationResult , target: Agent.Context.Self), waitFor: randomizedDuration);
            
        }

        private void FinishJob(IJobResult jobResult)
        {
            Agent.DebugMessage($"Finished Work with {_jobInProgress.Current.Name} {_jobInProgress.Current.Key} take next...");
            jobResult = jobResult.FinishedAt(Agent.CurrentTime);

            Agent.Send(BasicInstruction.FinishJob.Create(message: jobResult, target: _jobInProgress.Current.HubAgent));
            _jobInProgress.Reset();

            // then requeue processing queue if the item was delayed 
            if (jobResult.OriginalDuration != Agent.CurrentTime - jobResult.Start)
                RequeueAllRemainingJobs();

            // Do Work
            DoWork();
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

        */
    }
}
