using System;
using System.Collections.Generic;
using System.Linq;
using static IJobs;
using static FBuckets;
using static FJobConfirmations;
using Master40.SimulationCore.Agents.JobAgent;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobQueueItemLimited : LimitedQueue
    {

        public JobQueueItemLimited(int limit) : base(limit: limit)
        {
            
        }

        public bool Enqueue(FJobConfirmation jobConfirmation)
        {
            if (Limit <= JobConfirmations.Count) return false;
            JobConfirmations.Add(jobConfirmation);
            return true;
        }

        public bool EnqueueAll(List<FJobConfirmation> jobConfirmations)
        {
            if (Limit <= JobConfirmations.Count) return false;
            foreach (var job in jobConfirmations)
            {
                JobConfirmations.Add(job);
            }
            return true;
        }

        public override bool CapacitiesLeft()
        {
            return JobConfirmations.Count < Limit;
        }

        public long SumDurations => this.JobConfirmations.Sum(x => x.Job.Duration);

        internal bool Replace(FJobConfirmation jobConfirmation)
        {
            var jobToReplace = JobConfirmations.FirstOrDefault(x => x.Job.Key == jobConfirmation.Job.Key);
            if (jobToReplace == null) return false; 

            JobConfirmations.Remove(jobToReplace);
            return Enqueue(jobConfirmation);

        }

        internal bool Contains(Guid jobKey)
        {
            var jobToReplace = JobConfirmations.FirstOrDefault(x => x.Job.Key == jobKey);
            if (jobToReplace == null) return false;
            return true;
        }

        internal FJobConfirmation DequeueFirstSatisfiedFix(long currentTime)
        {
            var item = this.JobConfirmations.Where(x => x.Job.StartConditions.Satisfied && ((FBucket)x.Job).IsFixPlanned)
                                                .OrderBy(keySelector: x => x.Job.Priority(currentTime))
                                                .FirstOrDefault();
            if (item != null)
            {
                JobConfirmations.Remove(item: item);
            }
            return item;
        }

        internal bool Remove(FJobConfirmation jobConfirmation)
        {
            return this.JobConfirmations.Remove(item: jobConfirmation);
        }

        internal bool Remove(Guid jobKey)
        {
            var job = JobConfirmations.Single(x => x.Job.Key.Equals(jobKey));
            return this.Remove(job);
        }

        internal void ForceAdd(FJobConfirmation jobConfirmation)
        {
            this.JobConfirmations.Add(jobConfirmation);
        }
    }
}
