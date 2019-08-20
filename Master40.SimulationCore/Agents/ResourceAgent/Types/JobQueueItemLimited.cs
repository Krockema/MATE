using System;
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

        public override bool CapacitiesLeft()
        {
            return jobs.Count < jobs.Capacity;
        }
    }
}
