using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static FBuckets;
using static FBucketScopes;
using static FJobConfirmations;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    /// <summary>
    /// Queue for Planing the Scopes
    /// TODO implement / deviate from JobQueueTimeLimted 
    /// </summary>
    public class JobQueueScopeLimited : LimitedQueue, IJobQueue
    {

        public JobQueueScopeLimited(int limit) : base(limit: limit)
        {

        }

        public override FJobConfirmation DequeueFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability = null)
        {
            var bucket = GetFirstSatisfied(currentTime, resourceCapability);
            if (bucket != null)
            {
                JobConfirmations.Remove(bucket);
            }
            return bucket;
        }

        public FJobConfirmation GetFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability)
        {
            var buckets = JobConfirmations.Where(x => ((FBucket)x.Job).HasSatisfiedJob.Equals(true)).ToList();

            var avgResourceSetupTime = resourceCapability != null ? resourceCapability.ResourceSetups.Sum(x => x.SetupTime) : 0;
            var capabilityName = "";
            if (resourceCapability != null)
                capabilityName = resourceCapability.Name;
            
            var bucket = buckets.OrderBy(x => x.Job.Priority(currentTime) + x.Job.RequiredCapability.Name != capabilityName ? avgResourceSetupTime : 0).FirstOrDefault();
            
            return bucket;
        }

        public void Enqueue(FJobConfirmation jobConfirmation)
        {
            JobConfirmations.Add(item: jobConfirmation);
        }

        internal bool RemoveJob(FJobConfirmation jobConfirmation)
        {
            return JobConfirmations.Remove(item: jobConfirmation);
        }

        public override bool HasQueueAbleJobs()
        {
            var hasSatisfiedJob = false;
            foreach (var job in JobConfirmations)
            {
                if (((FBucket)job.Job).HasSatisfiedJob)
                {
                    hasSatisfiedJob = true;
                }
            }

            return hasSatisfiedJob;
        }

        public QueueingPosition GetQueueAbleTime(IJob job, long currentTime, long processingQueueLength, long resourceIsBlockedUntil, long setupDuration = 0)
        {
            var queuePosition = new QueueingPosition { EstimatedStart = currentTime + processingQueueLength + setupDuration };
            var totalWorkLoad = 0L;
            if (resourceIsBlockedUntil != 0)
                queuePosition.EstimatedStart = resourceIsBlockedUntil;

            //System.Diagnostics.Debug.WriteLine($"{job.Name} with priority {job.Priority(currentTime)}");
            if (JobConfirmations.Any(e => e.Job.Priority(currentTime) <= job.Priority(currentTime)))
            {
                var higherPrioBuckets = JobConfirmations.Where(e => e.Job.Priority(currentTime) <= job.Priority(currentTime))   
                    .ToList();
                /*TODO just debugging
                foreach (var bucket in higherPrioBuckets)
                {
                    System.Diagnostics.Debug.WriteLine($"{bucket.Name} has higher priority with {((IJob)bucket).Priority(currentTime)}");
                }*/
                //totalWorkLoad = higherPrioBuckets.Cast<IJob>().ToList().Where(x => x.StartConditions.Satisfied.Equals(true)).Sum(x => x.Duration);
                totalWorkLoad = higherPrioBuckets.Where(x => x.Job.StartConditions.Satisfied.Equals(true)).Sum(x => ((FBucket)x.Job).Scope);
                var totalEstimatedWorkload = higherPrioBuckets.ToList().Sum(x => x.Job.Duration);
                queuePosition.EstimatedStart += totalWorkLoad;
                queuePosition.EstimatedWorkload = totalEstimatedWorkload;
            }

            if (totalWorkLoad < Limit || ((FBucket)job).HasSatisfiedJob)
                queuePosition.IsQueueAble = true;

            //System.Diagnostics.Debug.WriteLine($"{job.Name} has queuePosition {queuePosition.EstimatedStart}");
            return queuePosition;
        }

        public HashSet<FJobConfirmation> CutTail(long currentTime, FJobConfirmation jobConfirmation)
        {
            // queued before another item?
            var toRequeue = new HashSet<FJobConfirmation>();
            var orderedList = JobConfirmations.OrderBy(x => x.Job.Priority(currentTime)).ToList();
            var position = orderedList.IndexOf(jobConfirmation);

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < JobConfirmations.Count)
            {
                toRequeue = orderedList.GetRange(index: position + 1,
                    count: JobConfirmations.Count() - position - 1).ToHashSet();
            }

            return toRequeue;
        }

        public override bool CapacitiesLeft()
        {
            return Limit > JobConfirmations.Cast<FBucket>().ToList().Sum(selector: x => x.Scope);
        }
    }
}
