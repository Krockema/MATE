using System;
using System.Collections.Generic;
using System.Linq;
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


        public void Enqueue(IJob job)
        {
            jobs.Add(item: job);
        }


        //TODO to be tested
        internal bool BucketScopeHasSatisfiedJob()
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

        internal QueueingPosition GetQueueAbleTime(IJob job, long currentTime, long processingQueueLength, long setupDuration, long resourceIsBlockedUntil)
        {
            var queuePosition = new QueueingPosition { EstimatedStart = currentTime + processingQueueLength + setupDuration };
            var totalWorkLoad = 0L;
            if (resourceIsBlockedUntil != 0)
                queuePosition.EstimatedStart = resourceIsBlockedUntil;

            if (this.jobs.Any(e => e.Priority(currentTime) <= job.Priority(currentTime)))
            {
                totalWorkLoad = this.jobs.Where(e => e.Priority(currentTime) <= job.Priority(currentTime))
                    .Sum(e => e.Duration);
                queuePosition.EstimatedStart += totalWorkLoad;
            }

            if (totalWorkLoad < Limit || job.StartConditions.Satisfied)
                queuePosition.IsQueueAble = true;
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

        internal void SetBucketFix(Guid bucketKey)
        {
            var bucket = (FBucket)jobs.SingleOrDefault(x => x.Key == bucketKey);
            if (bucket == null)
                throw new Exception($"Something went wrong");
            bucket = bucket.SetFixPlanned;
            //TODO to be tested
        }

        public override bool CapacitiesLeft()
        {
            throw new NotImplementedException();
        }
    }
}
