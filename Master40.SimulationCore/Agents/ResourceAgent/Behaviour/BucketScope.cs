using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBuckets;
using static FJobConfirmations;
using static FPostponeds;
using static FRequestProposalForCapabilityProviders;
using static FUpdateStartConditions;
using static IJobResults;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        public BucketScope(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider, SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType
        , planingJobQueueLength: planingJobQueueLength
        , fixedJobQueueSize: fixedJobQueueSize
        , workTimeGenerator: workTimeGenerator
        , capabilityProvider: capabilityProvider)
        {
        }

        private const int SCOPELIMIT = 960;

        //TODO PlaningQueueLenght as parameter
        private IJobQueue _scopeQueue = new TimeConstraintQueue(limit: SCOPELIMIT);

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
            var bucket = _scopeQueue.GetConfirmation(bucketKey);

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

        internal override void SendProposalTo(FRequestProposalForCapabilityProvider requestProposal)
        {

             var queuePositions = _scopeQueue.GetQueueAbleTime(requestProposal
                                                                , currentTime: Agent.CurrentTime
                                                                , cpm: _capabilityProviderManager
                                                                , resourceBlockedUntil: _jobInProgress.ResourceIsBusyUntil + _processingQueue.SumDurations);

            //TODO Sets Postponed to calculated Duration of Bucket
            var fPostponed = new FPostponed(offset: queuePositions.First().IsQueueAble ? 0 : Convert.ToInt32(_scopeQueue.Workload * 0.8));
            var jobPrio = requestProposal.Job.Priority(Agent.CurrentTime);
            Agent.DebugMessage(msg: queuePositions.First().IsQueueAble
                ? $"Bucket: {requestProposal.Job.Name} {requestProposal.Job.Key} IsQueueAble: {queuePositions.First().IsQueueAble} with EstimatedStart: {queuePositions.First().Start} and Prio: {jobPrio}"
                : $"Bucket: {requestProposal.Job.Name} {requestProposal.Job.Key} Postponed: {fPostponed.IsPostponed} with Offset: {fPostponed.Offset} and Prio: {jobPrio} ", CustomLogger.PROPOSAL, LogLevel.Warn);

            // calculate proposal
            var proposal = new FProposals.FProposal(possibleSchedule: queuePositions
                , postponed: fPostponed
                , requestProposal.CapabilityProviderId
                , resourceAgent: Agent.Context.Self
                , jobKey: requestProposal.Job.Key);

            Agent.Send(instruction: Hub.Instruction.Default.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }

        internal override void UpdateStartCondition(FUpdateStartCondition startCondition)
        {
            var buckets = _scopeQueue.GetJobsAs<FBucket>();
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
            var jobPrio = jobItem.Priority(Agent.CurrentTime);
            Agent.DebugMessage(msg: $"Start Acknowledge proposal for: {jobItem.Name} {jobItem.Key} with scope from {fJobConfirmation.ScopeConfirmation.Start} to {fJobConfirmation.ScopeConfirmation.Start} and priority {jobPrio}", CustomLogger.PROPOSAL, LogLevel.Warn);

            var isQueueable = _scopeQueue.CheckScope(fJobConfirmation, Agent.CurrentTime);

            if (!isQueueable)
            {
                Agent.DebugMessage(msg: $"Stop Acknowledge proposal for: {jobItem.Name} {jobItem.Key} with jobPrio: {jobPrio} and start requeue", CustomLogger.PROPOSAL, LogLevel.Warn);
                Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(jobItem.Key, target: jobItem.HubAgent));
                return;
            }

            Agent.DebugMessage(msg: $"Accepted proposal on resource {Agent.Context.Self.Path.Name} and start enqueue {jobItem.Name} {jobItem.Key} ", CustomLogger.PROPOSAL, LogLevel.Warn);

            // cut Tail before Enque
            UpdateAndRequeuePlanedJobs(fJobConfirmation);
            
            _scopeQueue.Enqueue(fJobConfirmation);

            Agent.DebugMessage(msg: $"AcknowledgeProposal finished: {jobItem.Name} {jobItem.Key} {_scopeQueue.Count}", CustomLogger.PROPOSAL, LogLevel.Warn);
            
            UpdateProcessingQueue();
            TryToWork();
        }

        internal override void UpdateAndRequeuePlanedJobs(FJobConfirmation jobConfirmation)
        {
            Agent.DebugMessage(msg: $"Old scope queue length on {Agent.Context.Self.Path.Name}: " + _scopeQueue.Count);
            var toRequeue = _scopeQueue.CutTail(currentTime: Agent.CurrentTime, jobConfirmation);
            foreach (var job in toRequeue)
            {
                _scopeQueue.RemoveJob(job);
                Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(job.Job.Key, target: job.Job.HubAgent));
            }
            Agent.DebugMessage(msg: $"New scope queue length on {Agent.Context.Self.Path.Name}: " + _scopeQueue.Count);
        }

        internal override void UpdateProcessingQueue()
        {
            // take the next scope and make it fix 
            while (_processingQueue.CapacitiesLeft() && _scopeQueue.HasQueueAbleJobs())
            {
                var job = _scopeQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime, _capabilityProviderManager.GetCurrentUsedCapability());

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
            
            var setupDuration = GetSetupTime(_jobInProgress.Current.CapabilityProvider.Id);
            
            //Start setup if necessary 
            
            if (setupDuration > 0)
            {
                Agent.DebugMessage(msg:
                    $"Start with Setup for Job {_jobInProgress.Current.Job.Name}  Key: {_jobInProgress.Current.Job.Key} " +
                    $"Duration is {setupDuration} and start with Job at {Agent.CurrentTime + setupDuration}");
                
                _capabilityProviderManager.Mount(_jobInProgress.Current.Job.RequiredCapability.Id);
                //TODO ExpectedDuration might be different by randomize setupDuration (see WorktimeGenerator at JobDuration)
                var pubSetup = new FCreateSimulationResourceSetups.FCreateSimulationResourceSetup(
                                                                                expectedDuration: setupDuration,
                                                                                duration: setupDuration,
                                                                                start: Agent.CurrentTime,
                                                                                resource: Agent.Name,
                                                                                capabilityName: _jobInProgress.RequiredCapabilityName,
                                                                                setupId: _jobInProgress.SetupId); 
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

            var pub = new FUpdateSimulationJobs.FUpdateSimulationJob(job: operation
                                                                    , jobType: JobType.OPERATION
                                                                    , duration: randomizedWorkDuration
                                                                    , start: Agent.CurrentTime
                                                                    , resource: Agent.Name
                                                                    , bucket: bucket.Name
                                                                    , setupId: _jobInProgress.SetupId);
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
            var item = _scopeQueue.FirstOrNull();
            if (item != null)
            {
                UpdateAndRequeuePlanedJobs(item);
            }
        }
    }
}
