using System;
using System.Collections.Generic;
using System.Linq;
using static IJobs;

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

        internal void Replace(IJob job)
        {
            var jobToReplace = jobs.FirstOrDefault(x => x.Key == job.Key);
            if (jobToReplace == null) return;

            jobs.Remove(jobToReplace);
            Enqueue(job);
            
        }
    }
}
