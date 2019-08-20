using System;
using System.Linq;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobQueueTimeLimited : LimitedQueue
    {
        public JobQueueTimeLimited(int limit) : base(limit: limit)
        {
        }
        /// <summary>
        /// To Be Testet
        /// </summary>
        /// <param name="item"></param>
        public bool Enqueue(IJob item)
        {
            if (!item.StartConditions.Satisfied 
                && Limit <= this.jobs.Sum(selector: x => x.Duration)) return false;
            this.jobs.Add(item: item);
            return true;
        }

        /// <summary>
        /// Function to determine the earliest start in a given Queue regardless of the queue capacity
        /// </summary>
        /// <param name="job"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public long GetQueueAbleTime(IJob job, long currentTime)
        {

            var possibleStartTime = currentTime;
            if (this.jobs.Any(e => e.Priority(currentTime) <= job.Priority(currentTime)))
            {
                possibleStartTime = this.jobs.Where(e => e.Priority(currentTime) <= job.Priority(currentTime))
                                             .Max(e => e.End);
            }
            return possibleStartTime;
        }
    }
}
