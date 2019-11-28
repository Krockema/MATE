﻿using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using System;
using System.Linq;
using static FBuckets;
using static FUpdateStartConditions;
using static IJobResults;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        public BucketScope(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, ToolManager toolManager, SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType
        , planingJobQueueLength: planingJobQueueLength
        , fixedJobQueueSize: fixedJobQueueSize
        , workTimeGenerator: workTimeGenerator
        , toolManager: toolManager)
        {
        }

        private JobQueueScopeLimited _scopeQueue = new JobQueueScopeLimited(limit: 1000);

        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                case Resource.Instruction.Default.RequestProposal msg: RequestProposal(jobItem: msg.GetObjectFromMessage); break;
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

            var bucket = _scopeQueue.GetBucket(bucketKey);

            if (bucket == null) return; 

            var success = _scopeQueue.RemoveJob(bucket);
            
            if (success)
            {
                Agent.DebugMessage($"{bucket.Name} has been send to requeue");
                Agent.Send(Hub.Instruction.BucketScope.ResetBucket.Create(bucketKey, bucket.HubAgent));
            }

        }

        internal override void SendProposalTo(IJob jobItem)
        {
            var setupDuration = GetSetupTime(jobItem: jobItem);

            var queuePosition = _scopeQueue.GetQueueAbleTime(job: jobItem
                , currentTime: Agent.CurrentTime
                , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                , processingQueueLength: _processingQueue.SumDurations
                , setupDuration: setupDuration);

            if (queuePosition.IsQueueAble)
            {
                Agent.DebugMessage(msg: $"IsQueueable: {queuePosition.IsQueueAble} with EstimatedStart: {queuePosition.EstimatedStart}");
            }
            //TODO Sets Postponed to calculated Duration of Bucket
            var fPostponed = new FPostponeds.FPostponed(offset: queuePosition.IsQueueAble ? 0 : queuePosition.EstimatedWorkload);

            if (fPostponed.IsPostponed)
            {
                Agent.DebugMessage(msg: $"Postponed: { fPostponed.IsPostponed } with Offset: { fPostponed.Offset} ");
            }
            // calculate proposal
            var proposal = new FProposals.FProposal(possibleSchedule: queuePosition.EstimatedStart
                , postponed: fPostponed
                , resourceAgent: Agent.Context.Self
                , jobKey: jobItem.Key);

            Agent.Send(instruction: Hub.Instruction.Default.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }

        internal override void UpdateStartCondition(FUpdateStartCondition startCondition)
        {
            var bucket = _scopeQueue.jobs.Cast<FBucket>()
                .SingleOrDefault(x => x.Operations.Any(x => x.Key == startCondition.OperationKey));

            if (bucket == null)
            {
                Agent.DebugMessage($"Bucket is not in Queue anymore");
            }
            else
            {
                var operation = bucket.Operations.Single(x => x.Key == startCondition.OperationKey);
                operation.SetStartConditions(startCondition: startCondition);

                Agent.DebugMessage($"Operation {operation.Operation.Name} {operation.Key} in {bucket.Name} has been startCondition set to: {operation.StartConditions.Satisfied} with preCondition: {operation.StartConditions.PreCondition} and articlesProvided {operation.StartConditions.ArticlesProvided}");

            }
            
            UpdateProcessingQueue();
            TryToWork();
        }

        internal override void AcknowledgeProposal(IJob jobItem)
        {
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
                Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create((FBucket)jobItem, target: jobItem.HubAgent));
                return;
            }


            jobItem = jobItem.UpdateEstimations(queuePosition.EstimatedStart, Agent.Context.Self);
            _scopeQueue.Enqueue(jobItem);

            Agent.DebugMessage(msg: "AcknowledgeProposal Accepted Item: " + jobItem.Name + " with Id: " + jobItem.Key);
            UpdateAndRequeuePlanedJobs(jobItem: jobItem);
            UpdateProcessingQueue();
            TryToWork();
        }

        internal override void UpdateAndRequeuePlanedJobs(IJob jobItem)
        {
            Agent.DebugMessage(msg: "Old scope queue length = " + _scopeQueue.Count);
            var toRequeue = _scopeQueue.CutTail(currentTime: Agent.CurrentTime, job: jobItem);
            foreach (var job in toRequeue)
            {
                _scopeQueue.RemoveJob(job: job);
                Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create((FBucket)job, target: job.HubAgent));
            }
            Agent.DebugMessage(msg: "New scope queue length = " + _scopeQueue.Count);
        }

        internal override void UpdateProcessingQueue()
        {
            // take the next scope and make it fix 
            while (_processingQueue.CapacitiesLeft() && _scopeQueue.HasQueueAbleJobs())
            {
                var job = _scopeQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);

                var ok = _processingQueue.Enqueue(item: job);
                if (!ok)
                {
                    throw new Exception(message: "Something went wrong with ProcessingQueueing!");
                }

                Agent.DebugMessage(msg: $"Job to place in processingQueue: {job.Name} {job.Key} with satisfied: {job.StartConditions.Satisfied} Try to start processing.");
                
                Agent.DebugMessage(msg: $"Ask for fix {job.Name} {job.Key} at {Agent.Context.Self.Path.Name}");
                Agent.Send(instruction: Hub.Instruction.BucketScope.SetBucketFix.Create(key: job.Key, target: job.HubAgent));

            }

            Agent.DebugMessage(msg: $"Jobs ready to start: {_processingQueue.Count} Try to start processing.");
        }

        /// <summary>
        /// After new Job has been put into the ProcessingQueue
        /// </summary>
        /// <param name="job"></param>
        internal void AcknowledgeJob(IJob job)
        {
            var bucket = (FBucket)job;

            if (bucket == null)
            {
                Agent.DebugMessage($"{job.Name} doesn't exits and couldn't be acknowledged");
                _processingQueue.Remove(job);
                return;
            }

            _processingQueue.Replace(job);
           
            Agent.DebugMessage($"{bucket.Name} {bucket.Key} with {bucket.Operations.Count} operations has now been acknowledged");
            
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

            Agent.DebugMessage($"Bucket start {_jobInProgress.Current.Name} was set on {Agent.Context.Self.Path.Name}");
            DoSetup();
        }

        internal override void DoSetup()
        {
            //Start setup if necessary 
            var setupDuration = GetSetupTime(_jobInProgress.Current);

            if (setupDuration > 0)
            {
                Agent.DebugMessage(
                    msg:
                    $"Start with Setup for Job {_jobInProgress.Current.Name}  Key: {_jobInProgress.Current.Key} Duration is {setupDuration} and start with Job at {Agent.CurrentTime + setupDuration}");
                _toolManager.Mount(requiredResourceTool: _jobInProgress.Current.Tool);
                //TODO ExpectedDuration might be different by randomize setupDuration (see WorktimeGenerator at JobDuration)
                var pubSetup = new FCreateSimulationResourceSetups.FCreateSimulationResourceSetup(
                    expectedDuration: setupDuration, duration: setupDuration, start: Agent.CurrentTime, resource: Agent.Name, resourceTool: _jobInProgress.Current.Tool.Name);
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
            var bucket = ((FBucket)_jobInProgress.Current);

            Agent.DebugMessage(
                msg:
                $"Do Work(): {bucket.Name} {bucket.Key} with {bucket.Operations.Count} operations at resource {Agent.Context.Self.Path.Name}");
            //get first satisfied item with lowest priority in bucket
            var operation = bucket.Operations.OrderByDescending(prio => prio.DueTime)
                .FirstOrDefault(op => op.StartConditions.Satisfied);
            
            //else - finish operation
            Agent.DebugMessage(msg: $"Start withdraw for article {operation.Operation.Name} {operation.Key}");
            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: operation.Key, target: _jobInProgress.Current.HubAgent));

            var randomizedWorkDuration = _workTimeGenerator.GetRandomWorkTime(duration: operation.Operation.Duration);
            Agent.DebugMessage(msg: $"Starting Job {operation.Operation.Name}  Key: {operation.Key} new Duration is {randomizedWorkDuration}");

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
            var bucket = (FBucket)_jobInProgress.Current;
            var operation = bucket.Operations.Single(x => x.Key == jobResult.Key);
            _jobInProgress.RemoveOperation(operation);
            Agent.DebugMessage($"Resource {Agent.Context.Self.Path.Name} called operation {operation.Operation.Name} {operation.Key} from bucket {bucket.Name} finished");

            Agent.Send(BasicInstruction.FinishJob.Create(message: jobResult, target: _jobInProgress.Current.HubAgent));

            var nextOperation = ((FBucket)_jobInProgress.Current).Operations.OrderByDescending(prio => prio.DueTime)
                .FirstOrDefault(op => op.StartConditions.Satisfied);
            //if there arent any operations - finish bucket
            if (nextOperation == null)
            {
                var fBucketResult = new FBucketResults.FBucketResult(key: _jobInProgress.Current.Key
                    , creationTime: 0
                    , start: _jobInProgress.StartTime
                    , end: Agent.CurrentTime
                    , originalDuration: _jobInProgress.Current.Duration
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
            Agent.DebugMessage(msg: $"Bucket finished work with {_jobInProgress.Current.Name} {_jobInProgress.Current.Key} take next...");
            
            Agent.Send(instruction: Hub.Instruction.BucketScope.FinishBucket.Create(jobResult: jobResult, target: _jobInProgress.Current.HubAgent));

            _jobInProgress.Reset();
            
            // then requeue processing queue if the item was delayed 
            if (jobResult.OriginalDuration != Agent.CurrentTime - jobResult.Start)
                //RequeueAllRemainingJobs();

            TryToWork();
        }

        internal override void RequeueAllRemainingJobs()
        {
            Agent.DebugMessage(msg: "Start to Requeue all remaining Jobs");
            var item = _scopeQueue.jobs.FirstOrDefault();
            if (item != null)
            {
                UpdateAndRequeuePlanedJobs(jobItem: item);
            }
        }
    }
}
