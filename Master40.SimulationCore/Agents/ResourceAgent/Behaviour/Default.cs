using Akka.Actor;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Types;
using System;
using System.Linq;
using Master40.SimulationCore.Helper.DistributionProvider;
using static FOperationResults;
using static FPostponeds;
using static FProposals;
using static FResourceInformations;
using static FUpdateSimulationJobs;
using static FUpdateStartConditions;
using static IJobResults;
using static IJobs;
using static FRequestProposalForSetups;
using static FJobConfirmations;
using static FOperations;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        public Default(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, ToolManager toolManager, SimulationType simulationType = SimulationType.None) : base(childMaker: null, simulationType: simulationType)
        {
            this._processingQueue = new JobQueueItemLimited(limit: fixedJobQueueSize);
            this._planingQueue = new JobQueueTimeLimited(limit: planingJobQueueLength);
            this._agentDictionary = new AgentDictionary();
            _workTimeGenerator = workTimeGenerator;
            _toolManager = toolManager;
        }
        // TODO Implement a JobManager
        internal JobQueueTimeLimited _planingQueue { get; set; }
        internal JobQueueItemLimited _processingQueue { get; set; }
        internal JobInProgress _jobInProgress { get; set; } = new JobInProgress();
        internal WorkTimeGenerator _workTimeGenerator { get; }
        internal ToolManager _toolManager { get; }
        internal AgentDictionary _agentDictionary { get; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Resource.Instruction.Default.SetHubAgent msg: SetHubAgent(hubAgent: msg.GetObjectFromMessage.Ref); break;
                case Resource.Instruction.Default.RequestProposal msg: RequestProposal(msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.AcknowledgeProposal msg: AcknowledgeProposal(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateStartCondition(startCondition: msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                // case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        public override bool AfterInit()
        {
            Agent.Send(instruction: Hub.Instruction.Default.AddResourceToHub.Create(message: 
                new FResourceInformation(
                    _toolManager.GetAllSetups()
                , requiredFor: Agent.Name
                , @ref: Agent.Context.Self)
                , target: Agent.VirtualParent));
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
        /// <param name="jobItem"></param>
        private void RequestProposal(FRequestProposalForSetup requestProposal)
        {
            Agent.DebugMessage(msg: $"Asked by Hub for Proposal: " + requestProposal.Job.Name + " with Id: " + requestProposal.Job.Key + " for SetupId " + requestProposal.SetupId);

            SendProposalTo(requestProposal);
        }

        /// <summary>
        /// Send Proposal to Hub Client
        /// </summary>
        /// <param name="jobItem"></param>
        internal void SendProposalTo(FRequestProposalForSetup requestProposal)
        {
            var queuePosition = _planingQueue.GetQueueAbleTime(job: requestProposal.Job
                                                     , currentTime: Agent.CurrentTime
                                          , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                                            , processingQueueLength: _processingQueue.SumDurations);

            Agent.DebugMessage(msg: $"IsQueueable: {queuePosition.IsQueueAble} with EstimatedStart: {queuePosition.EstimatedStart}");

            var fPostponed = new FPostponed(offset: queuePosition.IsQueueAble ? 0 : _planingQueue.Limit);

            Agent.DebugMessage(msg: $"Postponed: { fPostponed.IsPostponed } with Offset: { fPostponed.Offset} ");
            // calculate proposal
            var proposal = new FProposal(possibleSchedule: queuePosition.EstimatedStart
                , postponed: fPostponed
                , requestProposal.SetupId
                , resourceAgent: Agent.Context.Self
                , jobKey: requestProposal.Job.Key);

            Agent.Send(instruction: Hub.Instruction.Default.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }

        /// <summary>
        /// Is called after RequestProposal if the proposal is accepted by HubAgent
        /// </summary>
        public void AcknowledgeProposal(FJobConfirmation acknowledgeProposal)
        {
            Agent.DebugMessage(msg: $"Start Acknowledge proposal for: {acknowledgeProposal.Job.Name} {acknowledgeProposal.Job.Key}");

            var queuePosition = _planingQueue.GetQueueAbleTime(job: acknowledgeProposal.Job
                                                     , currentTime: Agent.CurrentTime
                                          , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                                           , processingQueueLength: _processingQueue.SumDurations);
            // if not QueueAble
            if (!queuePosition.IsQueueAble)
            {
                Agent.DebugMessage(msg: $"Stop Acknowledge proposal for: {acknowledgeProposal.Job.Name} {acknowledgeProposal.Job.Key} and start requeue");
                Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(message: acknowledgeProposal.Job, target: acknowledgeProposal.Job.HubAgent));
                return;
            }

            _planingQueue.Enqueue(item: acknowledgeProposal.Job);

            Agent.DebugMessage(msg: "AcknowledgeProposal Accepted Item: " + acknowledgeProposal.Job.Name + " with Id: " + acknowledgeProposal.Job.Key);
            UpdateAndRequeuePlanedJobs(jobItem: acknowledgeProposal.Job);
            UpdateProcessingQueue();
            DoWork();
        }

        private void RequeueAllRemainingJobs()
        {
            Agent.DebugMessage(msg: "Start to Requeue all remaining Jobs");
            var item = _planingQueue.jobs.FirstOrDefault();
            if (item != null)
            {
                UpdateAndRequeuePlanedJobs(jobItem: item);
            }
        }


        private void UpdateAndRequeuePlanedJobs(IJob jobItem)
        {
            Agent.DebugMessage(msg: "Old planning queue length = " + _planingQueue.Count);
            var toRequeue = _planingQueue.CutTail(currentTime: Agent.CurrentTime, job: jobItem);
            foreach (var job in toRequeue)
            {
                _planingQueue.RemoveJob(job: job);
                Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(message: job, target: job.HubAgent));
            }
            Agent.DebugMessage(msg: "New planning queue length = " + _planingQueue.Count);
        }


        private void UpdateProcessingQueue()
        {
            while (_processingQueue.CapacitiesLeft() && _planingQueue.HasQueueAbleJobs())
            {
                var job = _planingQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);
                Agent.DebugMessage(msg: $"Job to place in processingQueue: {job.Key} {job.Name} Try to start processing.");
                var ok = _processingQueue.Enqueue(item: job);
                if (!ok)
                {
                    throw new Exception(message: "Something wen wrong with Queueing!");
                }
                //TODO Withdraw at ProcessingQueue or DoWork?
                Agent.DebugMessage(msg: $"Start withdraw for article {job.Name} {job.Key}");
                Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: job.Key, target: job.HubAgent));
            }

            Agent.DebugMessage(msg: $"Jobs ready to start: {_processingQueue.Count} Try to start processing.");
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
                Agent.DebugMessage(msg: "Im still working....");
                return; // Resource Agent is still working
            }

            var nextJobInProgress = _processingQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);
            // Wait if nothing more to do
            if (nextJobInProgress == null)
            {
                // No more work 
                Agent.DebugMessage(msg: "Nothing more Ready in Queue!");
                return;
            }

            UpdateProcessingQueue();

            _jobInProgress.Set(job: nextJobInProgress, currentTime: Agent.CurrentTime);

            var randomizedDuration = _workTimeGenerator.GetRandomWorkTime(duration: nextJobInProgress.Duration);
            Agent.DebugMessage(msg: $"Starting Job {nextJobInProgress.Name}  Key: {nextJobInProgress.Key} new Duration is {randomizedDuration}");

            var pub = new FUpdateSimulationJob(job: nextJobInProgress, duration: randomizedDuration, start: Agent.CurrentTime, resource: Agent.Name, jobType: JobType.OPERATION, bucket: String.Empty);
            Agent.Context.System.EventStream.Publish(@event: pub);

            var fOperationResult = new FOperationResult(key: nextJobInProgress.Key
                                             , creationTime: 0
                                                    , start: Agent.CurrentTime
                                                      , end: Agent.CurrentTime + randomizedDuration
                                         , originalDuration: nextJobInProgress.Duration
                                          , productionAgent: ((FOperation)nextJobInProgress).ProductionAgent
                                            , resourceAgent: Agent.Context.Self);

            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: fOperationResult, target: Agent.Context.Self), waitFor: randomizedDuration);

        }

        private void FinishJob(IJobResult jobResult)
        {
            Agent.DebugMessage(msg: $"Finished Work with {_jobInProgress.Current.Name} {_jobInProgress.Current.Key} take next...");
            jobResult = jobResult.FinishedAt(Agent.CurrentTime);

            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: jobResult, target: _jobInProgress.Current.HubAgent));
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
