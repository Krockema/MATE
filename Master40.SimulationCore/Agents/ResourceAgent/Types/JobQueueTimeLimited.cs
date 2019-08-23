using System;
using System.Collections.Generic;
using System.Linq;
using static FUpdateStartConditions;
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
            if (!IsQueueAble(item: item)) return false;
            this.jobs.Add(item: item);
            return true;
        }

        public bool IsQueueAble(IJob item)
        {
            return item.StartConditions.Satisfied || CapacitiesLeft();
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

        public override bool CapacitiesLeft()
        {
            return Limit > this.jobs.Sum(selector: x => x.Duration);
        }

        public List<IJob> CutTail(long currentTime, IJob job)
        {
            // Enqued before another item?
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

        internal bool RemoveJob(IJob job)
        {
            return jobs.Remove(item: job);
        }

        internal bool UpdatePreCondition(FUpdateStartCondition startCondition)
        {
            var job = this.jobs.SingleOrDefault(x => x.Key == startCondition.OperationKey);
            if (job == null) return false;
            job.StartConditions.ArticlesProvided = startCondition.ArticlesProvided;
            job.StartConditions.PreCondition = startCondition.PreCondition;
            return job.StartConditions.ArticlesProvided && job.StartConditions.PreCondition;
        }
    }
}
