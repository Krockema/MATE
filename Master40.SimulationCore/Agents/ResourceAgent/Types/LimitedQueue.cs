using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public abstract class LimitedQueue
    {
        internal List<IJob> jobs = new List<IJob>();
        public int Limit { get; set; }
        public int Count => jobs.Count;

        public LimitedQueue(int limit)
        {
            Limit = limit;
        }

        /// <summary>
        /// Checks if there is an item ready to produce and removes it from queue and returns it to caller
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public IJob DequeueFirstSatisfied(long currentTime)
        {
            var item = this.jobs.Where(x => x.StartConditions.Satisfied).OrderBy(keySelector: x => x.Priority(currentTime)).FirstOrDefault();
            if (item != null)
            {
                this.jobs.Remove(item: item);
            }
            return item;
        }

        public abstract bool CapacitiesLeft();

        public bool HasQueueAbleJobs()
        {
            return this.jobs.Any(x => x.StartConditions.Satisfied);
        }

    }

}
