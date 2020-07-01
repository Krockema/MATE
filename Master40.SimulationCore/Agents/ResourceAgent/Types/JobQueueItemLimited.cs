using System;
using System.Collections.Generic;
using System.Linq;
using static IJobs;
using static FBuckets;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobQueueItemLimited : LimitedQueue
    {

        public JobQueueItemLimited(int limit) : base(limit: limit)
        {
            
        }

        public bool Enqueue(IJob item)
        {
            if (Limit <= this.jobs.Count) return false;
            this.jobs.Add(item: item);
            return true;
        }

        public bool EnqueueAll(List<IJob> jobs)
        {
            if (Limit <= this.jobs.Count) return false;
            this.jobs.AddRange(jobs);
            return true;
        }

        public override bool CapacitiesLeft()
        {
            return jobs.Count < Limit;
        }

        public long SumDurations => this.jobs.Sum(x => x.Duration);

        internal bool Replace(IJob job)
        {
            var jobToReplace = jobs.FirstOrDefault(x => x.Key == job.Key);
            if (jobToReplace == null) return false; 

            jobs.Remove(jobToReplace);
            return Enqueue(job);

        }

        internal IJob DequeueFirstSatisfiedFix(long currentTime)
        {
            var item = this.jobs.Cast<FBucket>().Where(x => x.StartConditions.Satisfied && x.IsFixPlanned).Cast<IJob>().OrderBy(keySelector: x => x.Priority(currentTime)).FirstOrDefault();
            if (item != null)
            {
                this.jobs.Remove(item: item);
            }
            return item;
        }

        internal bool Remove(IJob job)
        {
            return this.jobs.Remove(item: job);
        }

        internal bool Remove(Guid jobKey)
        {
            var job = this.jobs.Single(x => x.Key.Equals(jobKey));
            return this.Remove(job);
        }
    }
}
