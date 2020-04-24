using System;
using System.Collections.Generic;
using System.Linq;
using static FJobConfirmations;
using static FOperations;
using static FUpdateStartConditions;
using static IConfirmations;
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
        public void Enqueue(IConfirmation fJobConfirmations)
        {
           JobConfirmations.Add(fJobConfirmations);
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
        public QueueingPosition GetQueueAbleTime(IJob job,long currentTime, long resourceIsBlockedUntil, long processingQueueLength, long setupDuration = 0)
        {
            var queuePosition = new QueueingPosition {EstimatedStart = currentTime + processingQueueLength + setupDuration};
            var totalWorkLoad = 0L;
            if (resourceIsBlockedUntil != 0)
                queuePosition.EstimatedStart = resourceIsBlockedUntil;

            //TODO time scope for operation, not only priority based
            var estimatedWork = JobConfirmations.Where(e => e.Job.Priority(currentTime) <= job.Priority(currentTime));
            if (estimatedWork.Any())
            {
                totalWorkLoad = estimatedWork
                    .Sum(e => e.Job.Duration);
                queuePosition.EstimatedStart += totalWorkLoad;
            }

            if (totalWorkLoad < Limit || job.StartConditions.Satisfied)
                queuePosition.IsQueueAble = true;
            return queuePosition;
        }


        public QueueingPosition GetQueueAbleTimeByStack(IJob job, long currentTime, long resourceIsBlockedUntil, long processingQueueLength, long setupDuration = 0)
        {
            var queuePosition = new QueueingPosition { EstimatedStart = currentTime + processingQueueLength + setupDuration };
            var totalWorkLoad = 0L;
            if (resourceIsBlockedUntil != 0)
                queuePosition.EstimatedStart = resourceIsBlockedUntil;

            if (JobConfirmations.Any(e => e.Job.Priority(currentTime) <= job.Priority(currentTime)))
            {
                var allPreviousJobs = JobConfirmations.Where(e => e.Job.Priority(currentTime) <= job.Priority(currentTime)).ToList();
                var resourceTools = allPreviousJobs.Select(x => x.Job.RequiredCapability.Id).Distinct().ToList();
                var sumAllJobsWithToolId = JobConfirmations.Where(x => resourceTools.Contains(x.Job.RequiredCapability.Id)
                                                     && x.Job.RequiredCapability.Id != job.RequiredCapability.Id)
                                               .Sum(x => x.Job.Duration);

                var sumAllJobsWithSameToolId =
                    allPreviousJobs.Where(x => x.Job.RequiredCapability.Id == job.RequiredCapability.Id).Sum(x => x.Job.Duration);
                
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
            return Limit > JobConfirmations.Sum(selector: x => x.Job.Duration);
        }

        public HashSet<IConfirmation> CutTail(long currentTime, IConfirmation jobConfirmation)
        {
            // queued before another item?
            var toRequeue = new HashSet<IConfirmation>();
            var orderedList = JobConfirmations.OrderBy(x => x.Job.Priority(currentTime)).ToList();
            var position = orderedList.IndexOf(jobConfirmation);

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < JobConfirmations.Count)
            {
                toRequeue = orderedList.GetRange(index: position + 1, 
                                                 count: JobConfirmations.Count() - position - 1).ToHashSet();
            }

            return toRequeue;
        }

        public HashSet<IConfirmation> CutTailByStack(long currentTime, IConfirmation jobConfirmation)
        {
            var allPreviousJobs = JobConfirmations.Where(e => e.Job.Priority(currentTime) <= jobConfirmation.Job.Priority(currentTime)).ToList();
            var resourceTools = allPreviousJobs.Select(x => x.Job.RequiredCapability.Id).Distinct().ToList();

            var toRequeue = JobConfirmations.Where(e => e.Job.Priority(currentTime) > jobConfirmation.Job.Priority(currentTime)
                                                        && (!resourceTools.Contains(e.Job.RequiredCapability.Id) 
                                                        || e.Job.RequiredCapability.Id == jobConfirmation.Job.RequiredCapability.Id))
                                                            .ToHashSet();


            return toRequeue;
        }

        internal bool RemoveJob(IConfirmation jobConfirmation)
        {
            return JobConfirmations.Remove(item: jobConfirmation);
        }

        internal bool RemoveJob(Guid jobKey)
        {
            return 1 == JobConfirmations.RemoveWhere(x => x.Job.Key == jobKey);
        }

        internal bool UpdatePreCondition(FUpdateStartCondition startCondition)
        {
            var jobConfirmation = JobConfirmations.SingleOrDefault(x => x.Job.Key == startCondition.OperationKey);
            if (jobConfirmation == null) return false;
            ((FOperation)jobConfirmation.Job).SetStartConditions(startCondition.PreCondition, startCondition.ArticlesProvided);
            return jobConfirmation.Job.StartConditions.ArticlesProvided && jobConfirmation.Job.StartConditions.PreCondition;
        }
    }
}
