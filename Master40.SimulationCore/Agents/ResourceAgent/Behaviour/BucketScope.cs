using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using System;
using Master40.SimulationCore.Agents.HubAgent;
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

        /// <summary>
        /// default to 1000
        /// </summary>
        private JobQueueScopeLimited _scopeQueue = new JobQueueScopeLimited(limit: 1000);

        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                case Resource.Instruction.Default.RequestProposal msg: RequestProposal(jobItem: msg.GetObjectFromMessage); break;
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
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
            var fPostponed = new FPostponeds.FPostponed(offset: queuePosition.IsQueueAble ? 0 : _planingQueue.Limit);

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
                Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(message: jobItem, target: jobItem.HubAgent));
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
            Agent.DebugMessage(msg: "Old planning queue length = " + _scopeQueue.Count);
            var toRequeue = _scopeQueue.CutTail(currentTime: Agent.CurrentTime, job: jobItem);
            foreach (var job in toRequeue)
            {
                _planingQueue.RemoveJob(job: job);
                Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(message: job, target: job.HubAgent));
            }
            Agent.DebugMessage(msg: "New planning queue length = " + _planingQueue.Count);
        }

        internal override void UpdateProcessingQueue()
        {
            // take the next scope and make it fix 
            while (_processingQueue.CapacitiesLeft() && _scopeQueue.BucketScopeHasSatisfiedJob())
            {
                var job = _scopeQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);
                Agent.DebugMessage(msg: $"Job to place in processingQueue: {job.Key} {job.Name} Try to start processing.");
                var ok = _processingQueue.Enqueue(item: job);
                if (!ok)
                {
                    throw new Exception(message: "Something wen wrong with Queueing!");
                }
                Agent.DebugMessage(msg: $"Start withdraw for article {job.Name} {job.Key}");
                Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: job.Key, target: job.HubAgent));
            }

            Agent.DebugMessage(msg: $"Jobs ready to start: {_processingQueue.Count} Try to start processing.");
        }
        
    }
}
