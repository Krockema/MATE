using System;
using System.Linq;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobQueueTimeLimited : LimitedQueue
    {
        public JobQueueTimeLimited(int limit) : base(limit)
        {
        }
        /// <summary>
        /// To Be Testet
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(IJob item)
        {
            if (Limit > this.jobs.Sum(x => x.Duration))
                this.jobs.Add(item);
            else
                throw new Exception("Queue Limit Exeeds");
        }
    }
}
