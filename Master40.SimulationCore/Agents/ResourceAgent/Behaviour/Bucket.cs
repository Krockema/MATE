using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Types;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Master40.DB.Data.Helper;
using Master40.DB.DataModel;
using static FOperationResults;
using static FPostponeds;
using static FProposals;
using static FBuckets;
using static FUpdateSimulationWorks;
using static FUpdateStartConditions;
using static IJobResults;
using static IJobs;
using static FOperations;
using static FCreateSimulationResourceSetups;
using static FResourceInformations;
using static FBucketResults;
using static FRequestToRequeues;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class Bucket : SimulationCore.Types.Behaviour
    {
        public Bucket(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, ToolManager toolManager, SimulationType simulationType = SimulationType.None) : base(childMaker: null, simulationType: simulationType)
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
        internal ToolManager _toolManager { get; }
        internal WorkTimeGenerator _workTimeGenerator { get; }
        internal AgentDictionary _agentDictionary { get; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Resource.Instruction.SetHubAgent msg: SetHubAgent(hubAgent: msg.GetObjectFromMessage.Ref); break;
                case Resource.Instruction.RequestProposal msg: RequestProposal(jobItem: msg.GetObjectFromMessage); break;
                case Resource.Instruction.AcknowledgeProposal msg: AcknowledgeProposal(jobItem: msg.GetObjectFromMessage); break;
                case Resource.Instruction.BucketReady msg: BucketReady(msg.GetObjectFromMessage); break;
                case Resource.Instruction.DoWork msg: DoWork(); break;
                case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                case Resource.Instruction.FinishBucket msg: FinishBucket(msg.GetObjectFromMessage); break;
                case Resource.Instruction.RequestToRequeue msg: RequestRequeueBucket(msg.GetObjectFromMessage); break;
                case Resource.Instruction.EnqueueProcessingQueue msg: EnqueueProcessingQueue(msg.GetObjectFromMessage); break;
                // case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        public override bool AfterInit()
        {
            Agent.Send(instruction: Hub.Instruction.AddResourceToHub.Create(message: new FResourceInformation(resourceSetups: _toolManager.GetAllSetups()
                , requiredFor: Agent.Name, @ref: Agent.Context.Self), target: Agent.VirtualParent));
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
        private void RequestProposal(IJob jobItem)
        {
            Agent.DebugMessage(msg: $"Asked by Hub for Proposal: " + jobItem.Name + " with Id: " + jobItem.Key + ")");

            SendProposalTo(jobItem: jobItem);
        }

        /// <summary>
        /// Send Proposal to Hub Client
        /// </summary>
        /// <param name="jobItem"></param>
        internal void SendProposalTo(IJob jobItem)
        {
            var setupDuration = GetSetupTime(jobItem: jobItem);

            var queuePosition = _planingQueue.GetQueueAbleTime(job: jobItem
                                                     , currentTime: Agent.CurrentTime
                                          , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                                           , processingQueueLength: _processingQueue.SumDurations
                                                   , setupDuration: setupDuration);

            if (queuePosition.IsQueueAble)
            {
                Agent.DebugMessage(msg: $"IsQueueable: {queuePosition.IsQueueAble} with EstimatedStart: {queuePosition.EstimatedStart}");
            }
            var fPostponed = new FPostponed(offset: queuePosition.IsQueueAble ? 0 : _planingQueue.Limit);

            if (fPostponed.IsPostponed)
            {
                Agent.DebugMessage(msg: $"Postponed: { fPostponed.IsPostponed } with Offset: { fPostponed.Offset} ");
            }
            // calculate proposal
            var proposal = new FProposal(possibleSchedule: queuePosition.EstimatedStart
                , postponed: fPostponed
                , resourceAgent: Agent.Context.Self
                , jobKey: jobItem.Key);

            System.Diagnostics.Debug.WriteLine($"Job {jobItem.Key} IsQueueable: {queuePosition.IsQueueAble} with EstimatedStart: {queuePosition.EstimatedStart} at resource {Agent.Context.Self.Path.Name}");
            Agent.Send(instruction: Hub.Instruction.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }


        /// <summary>
        /// Is called after RequestProposal if the proposal is accepted by HubAgent
        /// </summary>
        public void AcknowledgeProposal(IJob jobItem)
        {
            Agent.DebugMessage(msg: $"Start Acknowledge proposal for: {jobItem.Name} {jobItem.Key} at resource {Agent.Context.Self.Path.Name}");

            var setupDuration = GetSetupTime(jobItem: jobItem);

            var queuePosition = _planingQueue.GetQueueAbleTime(job: jobItem
                                                     , currentTime: Agent.CurrentTime
                                          , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                                           , processingQueueLength: _processingQueue.SumDurations
                                                   , setupDuration: setupDuration);
            // if not QueueAble
            if (!queuePosition.IsQueueAble)
            {
                Agent.DebugMessage(msg: $"Stop Acknowledge proposal for: {jobItem.Name} {jobItem.Key} and start requeue");
                Agent.Send(instruction: Hub.Instruction.EnqueueBucket.Create(message: jobItem, target: jobItem.HubAgent));
                return;
            }

            jobItem = jobItem.UpdateEstimations(queuePosition.EstimatedStart, Agent.Context.Self);
            _planingQueue.Enqueue(item: jobItem);

            Agent.DebugMessage(msg: $"AcknowledgeProposal Accepted Item: {jobItem.Name} with Id: {jobItem.Key} at resource {Agent.Context.Self }");
            System.Diagnostics.Debug.WriteLine($"AcknowledgeProposal by {Agent.Context.Self.Path.Name} Accepted Item: {jobItem.Name} with {((FBucket)jobItem).Operations.Count} Id: {jobItem.Key}");

            UpdateAndRequeuePlanedJobs(jobItem: jobItem);
            UpdateProcessingQueue();
            TryToWork();
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
                Agent.Send(instruction: Hub.Instruction.EnqueueBucket.Create(message: job, target: job.HubAgent));
            }
            Agent.DebugMessage(msg: "New planning queue length = " + _planingQueue.Count);
        }


        private void UpdateProcessingQueue()
        {
            while (_processingQueue.CapacitiesLeft() && _planingQueue.HasQueueAbleJobs())
            {
                var job = _planingQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);
                Agent.DebugMessage(msg: $"Job to place in processingQueue: {job.Key} {job.Name} Try to start processing.");
                _processingQueue.Enqueue(item: job);
                //notify hub that bucket is in progress now
                Agent.Send(Hub.Instruction.SetJobFix.Create(message: job, target: job.HubAgent));
            }

            
            Agent.DebugMessage(msg: $"Buckets ready to start: {_processingQueue.Count} Try to start processing.");
        }

        private void EnqueueProcessingQueue(IJob job)
        {
            System.Diagnostics.Debug.WriteLine($"EnqueueProcessingQueue {job.Name} {job.Key}");

            _processingQueue.Replace(job);

            DoSetup();
        }



        private void BucketReady(Guid bucketKey)
        {
            Agent.DebugMessage(msg: $"UpdateArticleProvided for bucket: {bucketKey}");
            
            _planingQueue.SetBucketReady(bucketKey);
            UpdateProcessingQueue();
            TryToWork();
            
        }

        internal void TryToWork()
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

            _jobInProgress.Set(nextJobInProgress, Agent.CurrentTime);

            DoSetup();
        }

        private void DoSetup()
        {
            //Start setup if necessary 
            var setupDuration = GetSetupTime(_jobInProgress.Current);

            if (setupDuration > 0)
            {
                Agent.DebugMessage(
                    msg:
                    $"Start with Setup for Job {_jobInProgress.Current.Name}  Key: {_jobInProgress.Current.Key} Duration is {setupDuration} and start with Job at {Agent.CurrentTime + setupDuration}");
                _toolManager.Mount(requiredResourceTool: _jobInProgress.Current.Tool);
                var pubSetup = new FCreateSimulationResourceSetup(workScheduleId: _jobInProgress.Current.Key.ToString(),
                    duration: setupDuration, start: Agent.CurrentTime, resource: Agent.Name);
                Agent.Context.System.EventStream.Publish(@event: pubSetup);
            }

            _jobInProgress.SetStartTime(Agent.CurrentTime);

            System.Diagnostics.Debug.WriteLine($"Setup takes {setupDuration} {_jobInProgress.Current.Name} with {((FBucket)_jobInProgress.Current).Operations.Count} operation Id: {_jobInProgress.Current.Key} at resource {Agent.Context.Self}");

            Agent.Send(instruction: Resource.Instruction.DoWork.Create(message: null, target: Agent.Context.Self), waitFor: setupDuration);
        }

        /// <summary>
        /// Starts the next Job
        /// </summary>
        internal void DoWork()
        {
            //TODO for each operation in bucket try to work
            var bucket = ((FBucket)_jobInProgress.Current);
            System.Diagnostics.Debug.WriteLine($"Do Work(): {bucket.Name} {bucket.Key} with {bucket.Operations.Count} operations at resource {Agent.Context.Self.Path.Name}");

            //get first satisfied item with lowest priority in bucket
            var operation = bucket.Operations.OrderByDescending(prio => prio.DueTime)
                .FirstOrDefault(op => op.StartConditions.Satisfied);
            _jobInProgress.RemoveOperation(operation);

            //if there arent any operations - finish bucket
            if (operation == null)
            {
                var fBucketResult = new FBucketResult(key: _jobInProgress.Current.Key
                    , creationTime: 0
                    , start: _jobInProgress.StartTime
                    , end: Agent.CurrentTime
                    , originalDuration: _jobInProgress.Current.Duration
                    , productionAgent: ActorRefs.Nobody
                    , resourceAgent: Agent.Context.Self);

                System.Diagnostics.Debug.WriteLine($"Nothing more in bucket: {bucket.Name} with {bucket.Operations.Count} Id: {bucket.Key}");

                Agent.Send(instruction: Resource.Instruction.FinishBucket.Create(message: fBucketResult, target: Agent.Context.Self));
                return;
            }

            //else - finish operation
            System.Diagnostics.Debug.WriteLine($"Start withdraw for article {operation.Operation.Name} {operation.Key} at resource {Agent.Context.Self.Path.Name}");
            Agent.DebugMessage(msg: $"Start withdraw for article {operation.Operation.Name} {operation.Key}");
            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: operation.Key, target: _jobInProgress.Current.HubAgent));

            var randomizedWorkDuration = _workTimeGenerator.GetRandomWorkTime(duration: operation.Operation.Duration);
            Agent.DebugMessage(msg: $"Starting Job {operation.Operation.Name}  Key: {operation.Key} new Duration is {randomizedWorkDuration}");

            var pub = new FUpdateSimulationWork(workScheduleId: operation.Key.ToString(), duration: randomizedWorkDuration, start: Agent.CurrentTime, machine: Agent.Name);
            Agent.Context.System.EventStream.Publish(@event: pub);

            Agent.Send(instruction: Resource.Instruction.DoWork.Create(message: null, target: Agent.Context.Self),
                waitFor: randomizedWorkDuration);

            var fOperationResult = new FOperationResult(key: operation.Key
                , creationTime: 0
                , start: Agent.CurrentTime
                , end: Agent.CurrentTime + randomizedWorkDuration
                , originalDuration: operation.Operation.Duration
                , productionAgent: ActorRefs.Nobody
                , resourceAgent: Agent.Context.Self);

            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: fOperationResult, target: Agent.Context.Self), waitFor: randomizedWorkDuration);

        }

        /// <summary>
        /// source: gets request to delete job from queue
        /// target: feedback to hub, that job can be requeued now
        /// </summary>
        /// <param name="job">Job to Remove</param>
        private void RequestRequeueBucket(FRequestToRequeue requestToRequeue)
        {
            var jobToRequeue = _planingQueue.jobs.FirstOrDefault(x => x.Key == requestToRequeue.JobKey);
            if (jobToRequeue != null)
            {
                _planingQueue.RemoveJob(jobToRequeue);
                requestToRequeue = requestToRequeue.SetApproved;
                System.Diagnostics.Debug.WriteLine($"Asked to Requeue Bucket {jobToRequeue.Key} with {((FBucket)jobToRequeue).Operations.Count} operations");
            }

            Agent.Send(Hub.Instruction.ResponseRequeueBucket.Create(message: requestToRequeue, target: jobToRequeue.HubAgent));
        }

        private void FinishBucket(IJobResult jobResult)
        {
            System.Diagnostics.Debug.WriteLine($"Finished Work with {_jobInProgress.Current.Name} {_jobInProgress.Current.Key} at resource {Agent.Context.Self.Path.Name} take next...");

            Agent.DebugMessage(msg: $"Finished Work with {_jobInProgress.Current.Name} {_jobInProgress.Current.Key} take next...");
            jobResult = jobResult.FinishedAt(Agent.CurrentTime);

            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: jobResult, target: _jobInProgress.Current.HubAgent));
            _jobInProgress.Reset();

            // then requeue processing queue if the item was delayed 
            if (jobResult.OriginalDuration != Agent.CurrentTime - jobResult.Start)
                RequeueAllRemainingJobs();

            TryToWork();
        }

        private void FinishJob(IJobResult jobResult)
        {
            var bucket = (FBucket)_jobInProgress.Current;
            var operation = bucket.Operations.Single(x => x.Key == jobResult.Key);
            System.Diagnostics.Debug.WriteLine($"Resource {Agent.Context.Self.Path.Name} called Item  {operation.Key} from bucket {bucket.Name} finished");
            
            DoWork();
        }

        private int GetSetupTime(IJob jobItem)
        {
            var setupTime = 0;
            if (!_toolManager.AlreadyEquipped(requiredResourceTool: jobItem.Tool))
            {
                setupTime = _toolManager.GetSetupDurationByTool(resourceTool: jobItem.Tool);
            }

            Agent.DebugMessage(
                msg: $"Equipped Tool: {_toolManager.GetToolName()} | require Tool: {jobItem.Tool.Name} with setupDuration {setupTime}");
            return setupTime;
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
