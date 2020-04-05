using Akka.Actor;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using System.Linq;
using static FBuckets;
using static FJobConfirmations;
using static FRequestProposalForSetups;
using static FUpdateStartConditions;
using static IJobResults;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        public BucketScope(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, SetupManager toolManager, SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType
        , planingJobQueueLength: planingJobQueueLength
        , fixedJobQueueSize: fixedJobQueueSize
        , workTimeGenerator: workTimeGenerator
        , toolManager: toolManager)
        {
        }

        private const int SCOPELIMIT = 960;

        //TODO PlaningQueueLenght as parameter
        private JobQueueScopeLimited _scopeQueue = new JobQueueScopeLimited(limit: SCOPELIMIT);

        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                case Resource.Instruction.Default.RequestProposal msg: RequestProposal(msg.GetObjectFromMessage); break;
                case Resource.Instruction.BucketScope.RequeueBucket msg: RequeueBucket(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateStartCondition(msg.GetObjectFromMessage); break;
                case Resource.Instruction.BucketScope.AcknowledgeJob msg: AcknowledgeJob(msg.GetObjectFromMessage); break;
                case Resource.Instruction.BucketScope.FinishBucket msg: FinishBucket(msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(msg.GetObjectFromMessage); break;
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }

        private void RequeueBucket(Guid bucketKey)
        {
            //TODO not working with multiresource  fix
            var bucket = _scopeQueue.JobConfirmations.SingleOrDefault(x => x.Job.Key == bucketKey);

            if (bucket == null) return;

            //receive all acknowledge resets and send reset to hub / Bucket manager

            var success = _scopeQueue.RemoveJob(bucket);

            if (success)
            {
                Agent.DebugMessage($"{bucket.Job.Name} has been send to requeue");
                Agent.Send(Hub.Instruction.BucketScope.EnqueueBucket.Create(bucketKey, bucket.Job.HubAgent));
            }
            // send to all other resources
            // response to AcknowledgeReset

        }

        internal void AcknowledgeReset(Guid bucketKey)
        {
        }

        internal override void SendProposalTo(FRequestProposalForSetup requestProposal)
        {
            var setupDuration = GetSetupTime(jobItem: requestProposal.Job);

            var queuePosition = _scopeQueue.GetQueueAbleTime(job: requestProposal.Job
                , currentTime: Agent.CurrentTime
                , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                , processingQueueLength: _processingQueue.SumDurations
                , setupDuration: setupDuration);

            //TODO Sets Postponed to calculated Duration of Bucket
            var fPostponed = new FPostponeds.FPostponed(offset: queuePosition.IsQueueAble ? 0 : Convert.ToInt32(SCOPELIMIT * 0.8));

            Agent.DebugMessage(msg: queuePosition.IsQueueAble
                ? $"Bucket: {requestProposal.Job.Key} IsQueueAble: {queuePosition.IsQueueAble} with EstimatedStart: {queuePosition.EstimatedStart}"
                : $"Bucket: {requestProposal.Job.Key} Postponed: {fPostponed.IsPostponed} with Offset: {fPostponed.Offset} ");

            // calculate proposal
            var proposal = new FProposals.FProposal(possibleSchedule: queuePosition.EstimatedStart
                , postponed: fPostponed
                , requestProposal.SetupId
                , resourceAgent: Agent.Context.Self
                , jobKey: requestProposal.Job.Key);

            Agent.Send(instruction: Hub.Instruction.Default.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }

        internal override void UpdateStartCondition(FUpdateStartCondition startCondition)
        {
            var buckets = _scopeQueue.JobConfirmations.Select(x => x.Job).Cast<FBucket>();
            var bucket = buckets?.SingleOrDefault(x => x.Operations.Any(x => x.Key == startCondition.OperationKey));
            if (bucket != null)
            {
                var operation = bucket.Operations.Single(x => x.Key == startCondition.OperationKey);
                operation.SetStartConditions(startCondition: startCondition);
                Agent.DebugMessage($"Operation {operation.Operation.Name} {operation.Key} in {bucket.Name} has been startCondition set to: {operation.StartConditions.Satisfied} with preCondition: {operation.StartConditions.PreCondition} and articlesProvided {operation.StartConditions.ArticlesProvided}");
            }
            else
            {
                Agent.DebugMessage($"Bucket is not in Queue anymore");
            }
            
            UpdateProcessingQueue();
            TryToWork();
        }

        internal override void AcknowledgeProposal(FJobConfirmation fJobConfirmation)
        {
            var jobItem = fJobConfirmation.Job;
            Agent.DebugMessage(msg: $"Start Acknowledge proposal for: {jobItem.Name} {jobItem.Key}");

            var setupDuration = GetSetupTime(jobItem: jobItem);

            var queuePosition = _scopeQueue.GetQueueAbleTime(job: jobItem
                , currentTime: Agent.CurrentTime
                , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                , processingQueueLength: _processingQueue.SumDurations
                , setupDuration: setupDuration);
            // if not QueueAble
            if (!queuePosition.IsQueueAble)
            {
                Agent.DebugMessage(msg: $"Stop Acknowledge proposal for: {jobItem.Name} {jobItem.Key} and start requeue");
                Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(jobItem.Key, target: jobItem.HubAgent));
                return;
            }
            _scopeQueue.Enqueue(fJobConfirmation);

            Agent.DebugMessage(msg: "AcknowledgeProposal Accepted Item: " + jobItem.Name + " with Id: " + jobItem.Key);
            UpdateAndRequeuePlanedJobs(fJobConfirmation);
            UpdateProcessingQueue();
            TryToWork();
        }

        internal override void UpdateAndRequeuePlanedJobs(FJobConfirmation jobConfirmation)
        {
            Agent.DebugMessage(msg: "Old scope queue length = " + _scopeQueue.Count);
            var toRequeue = _scopeQueue.CutTail(currentTime: Agent.CurrentTime, jobConfirmation);
            foreach (var job in toRequeue)
            {
                _scopeQueue.RemoveJob(job);
                Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(job.Job.Key, target: job.Job.HubAgent));
            }
            Agent.DebugMessage(msg: "New scope queue length = " + _scopeQueue.Count);
        }

        internal override void UpdateProcessingQueue()
        {
            // take the next scope and make it fix 
            while (_processingQueue.CapacitiesLeft() && _scopeQueue.HasQueueAbleJobs())
            {
                var job = _scopeQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime, _setupManager.GetCurrentUsedCapability());

                var ok = _processingQueue.Enqueue(job);
                if (!ok)
                {
                    throw new Exception(message: "Something went wrong with ProcessingQueueing!");
                }

                Agent.DebugMessage(msg: $"Job to place in processingQueue: {job.Job.Name} {job.Job.Key} with satisfied: {job.Job.StartConditions.Satisfied} Try to start processing.");
                
                Agent.DebugMessage(msg: $"Ask for fix {job.Job.Name} {job.Job.Key} at {Agent.Context.Self.Path.Name}");
                Agent.Send(instruction: Hub.Instruction.BucketScope.SetBucketFix.Create(key: job.Job.Key, target: job.Job.HubAgent));
            }

            Agent.DebugMessage(msg: $"Jobs ready to start: {_processingQueue.Count} Try to start processing.");
        }

        /// <summary>
        /// After new Job has been put into the ProcessingQueue
        /// </summary>
        /// <param name="job"></param>
        internal void AcknowledgeJob(FJobConfirmation jobConfirmation)
        {
            if (jobConfirmation.IsReset)
            {
                Agent.DebugMessage($"Bucket {jobConfirmation.Job.Key} doesn't exits and couldn't be acknowledged");
                _processingQueue.Remove(jobConfirmation);
                UpdateProcessingQueue();
                return;
            }


            if (!_processingQueue.Replace(jobConfirmation)) 
                throw new Exception("Could not find Job in Processing Queue");
           
            Agent.DebugMessage($"{jobConfirmation.Job.Name} {jobConfirmation.Job.Key} with {((FBucket)jobConfirmation.Job).Operations.Count} operations has now been acknowledged");
            
            TryToWork();
        }

        internal override void TryToWork()
        {
            if (_jobInProgress.IsSet)
            {
                Agent.DebugMessage(msg: "Im still working....");
                return; // Resource Agent is still working
            }

            var nextJobInProgress = _processingQueue.DequeueFirstSatisfiedFix(currentTime: Agent.CurrentTime);

            // Wait if nothing more to do
            if (nextJobInProgress == null)
            {
                // No more work 
                Agent.DebugMessage(msg: "Nothing more Ready in Queue!");
                return;
            }
            
            UpdateProcessingQueue();

            _jobInProgress.Set(nextJobInProgress, Agent.CurrentTime);

            Agent.DebugMessage($"Bucket start {_jobInProgress.Current.Job.Name} was set on {Agent.Context.Self.Path.Name}");
            DoSetup();
        }

        internal override void DoSetup()
        {
            //Start setup if necessary 
            var setupDuration = GetSetupTime(_jobInProgress.Current.Job);

            if (setupDuration > 0)
            {
                Agent.DebugMessage(msg:
                    $"Start with Setup for Job {_jobInProgress.Current.Job.Name}  Key: {_jobInProgress.Current.Job.Key} " +
                    $"Duration is {setupDuration} and start with Job at {Agent.CurrentTime + setupDuration}");
                
                _setupManager.Mount(_jobInProgress.Current.Job.RequiredCapability);
                //TODO ExpectedDuration might be different by randomize setupDuration (see WorktimeGenerator at JobDuration)
                var pubSetup = new FCreateSimulationResourceSetups.FCreateSimulationResourceSetup(
                                                                                expectedDuration: setupDuration,
                                                                                duration: setupDuration,
                                                                                start: Agent.CurrentTime,
                                                                                resource: Agent.Name,
                                                                                capabilityName: _jobInProgress.Current.Job.RequiredCapability.Name); 
                Agent.Context.System.EventStream.Publish(@event: pubSetup);
            }

            _jobInProgress.SetStartTime(Agent.CurrentTime);

            Agent.Send(instruction: Resource.Instruction.Default.DoWork.Create(message: null, target: Agent.Context.Self), waitFor: setupDuration);
        }

        /// <summary>
        /// Starts the next Job
        /// </summary>
        internal override void DoWork()
        {
            Agent.DebugMessage("Call start Job");
            //TODO for each operation in bucket try to work
            var bucket = (FBucket)_jobInProgress.Current.Job;

            //get first satisfied item with lowest priority in bucket
            var operation = bucket.Operations.OrderByDescending(prio => prio.DueTime)
                .FirstOrDefault(op => op.StartConditions.Satisfied && !op.IsFinished);
            
            //else - finish operation
            Agent.DebugMessage(msg: $"Start withdraw for article {operation.Operation.Name} {operation.Key}");
            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: operation.Key, target: _jobInProgress.Current.Job.HubAgent));

            var randomizedWorkDuration = _workTimeGenerator.GetRandomWorkTime(duration: operation.Operation.Duration);
            Agent.DebugMessage(msg: $"Starting Job {operation.Operation.Name}  Key: {operation.Key} new Duration is {randomizedWorkDuration} " +
                                    $"from bucket {bucket.Name} {bucket.Key} with {bucket.Operations.Count} operations " +
                                    $"at resource {Agent.Context.Self.Path.Name}");

            var pub = new FUpdateSimulationJobs.FUpdateSimulationJob(job: operation, jobType: JobType.OPERATION, duration: randomizedWorkDuration, start: Agent.CurrentTime, resource: Agent.Name, bucket: bucket.Name);
            Agent.Context.System.EventStream.Publish(@event: pub);

            var fOperationResult = new FOperationResults.FOperationResult(key: operation.Key
                , creationTime: 0
                , start: Agent.CurrentTime
                , end: Agent.CurrentTime + randomizedWorkDuration
                , originalDuration: operation.Operation.Duration
                , productionAgent: ActorRefs.Nobody
                , resourceAgent: Agent.Context.Self);

            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: fOperationResult, target: Agent.Context.Self), waitFor: randomizedWorkDuration);

        }

        internal override void FinishJob(IJobResult jobResult)
        {
            Agent.DebugMessage("Call finish Job");
            var bucket = (FBucket)_jobInProgress.Current.Job;
            var operation = bucket.Operations.Single(x => x.Key == jobResult.Key);
            operation.SetFinished();
            Agent.DebugMessage($"Resource {Agent.Context.Self.Path.Name} called operation {operation.Operation.Name} {operation.Key} from bucket {bucket.Name} {bucket.Key} finished");

            Agent.Send(BasicInstruction.FinishJob.Create(message: jobResult, target: bucket.HubAgent));

            var nextOperation = bucket.Operations.OrderByDescending(prio => prio.DueTime)
                                                 .FirstOrDefault(op => op.StartConditions.Satisfied
                                                                   && !op.IsFinished); // Obsolete ?
            //if there arent any operations - finish bucket
            if (nextOperation == null)
            {
                var fBucketResult = new FBucketResults.FBucketResult(key: _jobInProgress.Current.Job.Key
                    , creationTime: 0
                    , start: _jobInProgress.StartTime
                    , end: Agent.CurrentTime
                    , originalDuration: _jobInProgress.Current.Job.Duration
                    , productionAgent: ActorRefs.Nobody
                    , resourceAgent: Agent.Context.Self);

                Agent.DebugMessage($"Nothing more in bucket: {bucket.Name} with {bucket.Operations.Count} Id: {bucket.Key}");

                Agent.Send(instruction: Resource.Instruction.BucketScope.FinishBucket.Create(message: fBucketResult, target: Agent.Context.Self));
                return;
            }

            DoWork();
        }

        internal void FinishBucket(IJobResult jobResult)
        {
            Agent.DebugMessage(msg: $"Bucket finished work with {_jobInProgress.Current.Job.Name} {_jobInProgress.Current.Job.Key} take next...");
            
            Agent.Send(instruction: Hub.Instruction.BucketScope.FinishBucket.Create(jobResult: jobResult, target: _jobInProgress.Current.Job.HubAgent));

            _jobInProgress.Reset();
            
            // then requeue processing queue if the item was delayed 
            if (jobResult.OriginalDuration != Agent.CurrentTime - jobResult.Start)
                RequeueAllRemainingJobs();

            TryToWork();
        }

        internal override void RequeueAllRemainingJobs()
        {
            Agent.DebugMessage(msg: "Start to Requeue all remaining Jobs");
            var item = _scopeQueue.JobConfirmations.FirstOrDefault();
            if (item != null)
            {
                UpdateAndRequeuePlanedJobs(item);
            }
        }
    }
}
