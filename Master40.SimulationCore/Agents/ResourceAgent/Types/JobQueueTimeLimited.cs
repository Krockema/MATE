using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using MessagePack.Resolvers;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
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
        public void Enqueue(IJob item)
        {
           this.jobs.Add(item: item);
            return;
        }

        /// <summary>
        /// Function to determine the earliest start in a given Queue regardless of the queue capacity
        /// </summary>
        /// <param name="job"></param>
        /// <param name="currentTime"></param>
        /// <param name="resourceIsBlockedUntil"></param>
        /// <param name="processingQueueLength"></param>
        /// <param name="setupDuration">optional parameter which adds the setupDuration to queable time</param>
        /// <returns></returns>
        public QueueingPosition GetQueueAbleTime(IJob job,long currentTime, long resourceIsBlockedUntil, long processingQueueLength, int setupDuration = 0)
        {
            var queuePosition = new QueueingPosition {EstimatedStart = currentTime + processingQueueLength + setupDuration};
            var totalWorkLoad = 0L;
            if (resourceIsBlockedUntil != 0)
                queuePosition.EstimatedStart = resourceIsBlockedUntil;

            if (this.jobs.Any(e => e.Priority(currentTime) <= job.Priority(currentTime)))
            {
                totalWorkLoad = this.jobs.Where(e => e.Priority(currentTime) <= job.Priority(currentTime))
                    .Sum(e => e.Duration);
                queuePosition.EstimatedStart += totalWorkLoad;
            }

            if (totalWorkLoad < Limit || job.StartConditions.Satisfied)
                queuePosition.IsQueueAble = true;
            return queuePosition;
        }

        public QueueingPosition GetQueueAbleTimeByStack(IJob job, long currentTime, long resourceIsBlockedUntil, long processingQueueLength, int setupDuration = 0)
        {
            var queuePosition = new QueueingPosition { EstimatedStart = currentTime + processingQueueLength + setupDuration };
            var totalWorkLoad = 0L;
            if (resourceIsBlockedUntil != 0)
                queuePosition.EstimatedStart = resourceIsBlockedUntil;

            if (this.jobs.Any(e => e.Priority(currentTime) <= job.Priority(currentTime)))
            {
                var allPreviousJobs = this.jobs.Where(e => e.Priority(currentTime) <= job.Priority(currentTime)).ToList();
                var resourceTools = allPreviousJobs.Select(x => x.Tool.Id).Distinct().ToList();
                var sumAllJobsWithToolId = jobs.Where(x => resourceTools.Contains(x.Tool.Id)
                                                     && x.Tool.Id != job.Tool.Id)
                                               .Sum(x => x.Duration);

                var sumAllJobsWithSameToolId =
                    allPreviousJobs.Where(x => x.Tool.Id == job.Tool.Id).Sum(x => x.Duration);
                
                totalWorkLoad = sumAllJobsWithSameToolId + sumAllJobsWithToolId;
                queuePosition.EstimatedStart += totalWorkLoad;

            }
            if (totalWorkLoad < Limit || job.StartConditions.Satisfied)
                queuePosition.IsQueueAble = true;
            return queuePosition;
        }

        /// <summary>
        /// Return true if queue limit is not reached
        /// </summary>
        /// <returns></returns>
        public override bool CapacitiesLeft()
        {
            return Limit > this.jobs.Sum(selector: x => x.Duration);
        }

        public List<IJob> CutTail(long currentTime, IJob job)
        {
            // queued before another item?
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

        public List<IJob> CutTailByStack(long currentTime, IJob job)
        {
            var allPreviousJobs = this.jobs.Where(e => e.Priority(currentTime) <= job.Priority(currentTime)).ToList();
            var resourceTools = allPreviousJobs.Select(x => x.Tool.Id).Distinct().ToList();

            var toRequeue = this.jobs.Where(e => e.Priority(currentTime) > job.Priority(currentTime)
                                                        && (!resourceTools.Contains(e.Tool.Id) 
                                                        || e.Tool.Id == job.Tool.Id))
                                                            .ToList();


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

        internal void SetBucketReady(Guid bucketKey)
        {
            var job = jobs.SingleOrDefault(x => x.Key == bucketKey);
            if (job == null) 
                throw new Exception($"Something went wrong");
            job.StartConditions.ArticlesProvided = true;
            job.StartConditions.PreCondition = true;
        }
    }
}
