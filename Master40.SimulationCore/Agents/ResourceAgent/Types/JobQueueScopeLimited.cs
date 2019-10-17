using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static FBuckets;
using static FBucketScopes;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    /// <summary>
    /// Queue for Planing the Scopes
    /// </summary>
    public class JobQueueScopeLimited : LimitedQueue
    {

        public JobQueueScopeLimited(int limit) : base(limit: limit)
        {

        }

        public override IJob DequeueFirstSatisfied(long currentTime)
        {
            var buckets = this.jobs.Cast<FBucket>().Where(x => x.HasSatisfiedJob.Equals(true)).ToList().Cast<IJob>();
            var bucket = buckets.OrderBy(x => x.Priority(currentTime)).FirstOrDefault(); 
            if (bucket != null)
            {
                this.jobs.Remove(bucket);
            }
            return bucket;
        }

        public void Enqueue(IJob job)
        {
            jobs.Add(item: job);
        }

        internal bool RemoveJob(IJob job)
        {
            return jobs.Remove(item: job);
        }

        public override bool HasQueueAbleJobs()
        {
            var hasSatisfiedJob = false;
            foreach (var job in jobs)
            {
                if (((FBucket) job).HasSatisfiedJob)
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

            System.Diagnostics.Debug.WriteLine($"{job.Name} with priority {job.Priority(currentTime)}");
            if (this.jobs.Any(e => e.Priority(currentTime) <= job.Priority(currentTime)))
            {
                var higherPrioBuckets = jobs.Where(e => e.Priority(currentTime) <= job.Priority(currentTime))   
                    .Cast<FBucket>().ToList();
                //TODO just debugging
                foreach (var bucket in higherPrioBuckets)
                {
                    System.Diagnostics.Debug.WriteLine($"{bucket.Name} has higher priority with {((IJob)bucket).Priority(currentTime)}");
                }
                totalWorkLoad = higherPrioBuckets.Sum(x => x.Scope);
                var totalEstimatedWorkload = higherPrioBuckets.Cast<IJob>().ToList().Sum(x => x.Duration);
                queuePosition.EstimatedStart += totalWorkLoad;
                queuePosition.EstimatedWorkload = totalEstimatedWorkload;
            }

            if (totalWorkLoad < Limit || ((FBucket)job).HasSatisfiedJob)
                queuePosition.IsQueueAble = true;

            System.Diagnostics.Debug.WriteLine($"{job.Name} has queuePosition {queuePosition.EstimatedStart}");
            return queuePosition;
        }

        public List<IJob> CutTail(long currentTime, IJob job)
        {
            // queued before another item?
            var toRequeue = new List<IJob>();
            var position = jobs.OrderBy(x => x.Priority(currentTime)).ToList().IndexOf(job);

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < jobs.Count)
            {
                toRequeue = jobs.OrderBy(x => x.Priority(currentTime)).ToList()
                    .GetRange(position + 1, jobs.Count() - position - 1);
            }

            return toRequeue;
        }

        public override bool CapacitiesLeft()
        {
            return Limit > this.jobs.Cast<FBucket>().ToList().Sum(selector: x => x.Scope);
        }

        public void UpdateBucket(IJob job)
        {
            RemoveJob(job);
            Enqueue(job);
        }

    }
}
