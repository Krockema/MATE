using System;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobQueueItemLimited : LimitedQueue
    {

        public JobQueueItemLimited(int limit) : base(limit)
        {
            
        }

        public void Enqueue(IJob item)
        {
            if (Limit > this.jobs.Count)
                this.jobs.Add(item);
            else
                throw new Exception("Queue Limit Exeeds");
        }
    }
}
