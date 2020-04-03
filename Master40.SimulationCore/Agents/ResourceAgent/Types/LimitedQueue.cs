using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FJobConfirmations;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public abstract class LimitedQueue
    {
        public HashSet<FJobConfirmation> JobConfirmations { get; } = new HashSet<FJobConfirmation>();
        public int Limit { get; set; }
        public int Count => JobConfirmations.Count;

        public LimitedQueue(int limit)
        {
            Limit = limit;
        }

        /// <summary>
        /// Checks if there is an item ready to produce and removes it from queue and returns it to caller
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public virtual FJobConfirmation DequeueFirstSatisfied(long currentTime, M_Resource equippdedResourceTool = null)
        {
            var item = this.JobConfirmations.Where(x => x.Job.StartConditions.Satisfied).OrderBy(keySelector: x => x.Job.Priority(currentTime)).FirstOrDefault();
            if (item != null)
            {
                this.JobConfirmations.Remove(item: item);
            }
            return item;
        }

        internal HashSet<FJobConfirmation> GetAllSatisfiedSameCapability(long currentTime)
        {
            var jobConfirmation = DequeueFirstSatisfied(currentTime);
            var list = this.JobConfirmations.Where(x => x.Job.StartConditions.Satisfied && x.Job.RequiredCapability.Id == jobConfirmation.Job.RequiredCapability.Id).ToHashSet();
            foreach (var item in list)
            {
                this.JobConfirmations.Remove(item: item);
            }
            list.Add(jobConfirmation);

            return list;
        }

        public abstract bool CapacitiesLeft();

        public virtual bool HasQueueAbleJobs()
        {
            return this.JobConfirmations.Any(x => x.Job.StartConditions.Satisfied);
        }

    }

}
