using System.Collections.Generic;
using System.Linq;
using static FJobConfirmations;
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
        public void Enqueue(FJobConfirmation fJobConfirmations)
        {
           _jobConfirmations.Add(fJobConfirmations);
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
            var estimatedWork = _jobConfirmations.Where(e => e.Job.Priority(currentTime) <= job.Priority(currentTime));
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

            if (_jobConfirmations.Any(e => e.Job.Priority(currentTime) <= job.Priority(currentTime)))
            {
                var allPreviousJobs = _jobConfirmations.Where(e => e.Job.Priority(currentTime) <= job.Priority(currentTime)).ToList();
                var resourceTools = allPreviousJobs.Select(x => x.Job.RequiredCapability.Id).Distinct().ToList();
                var sumAllJobsWithToolId = _jobConfirmations.Where(x => resourceTools.Contains(x.Job.RequiredCapability.Id)
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
            return Limit > _jobConfirmations.Sum(selector: x => x.Job.Duration);
        }

        public HashSet<FJobConfirmation> CutTail(long currentTime, FJobConfirmation jobConfirmation)
        {
            // queued before another item?
            var toRequeue = new HashSet<FJobConfirmation>();
            var orderedList = _jobConfirmations.OrderBy(x => x.Job.Priority(currentTime)).ToList();
            var position = orderedList.IndexOf(jobConfirmation);

            // reorganize Queue if an Element has ben Queued which is More Important.
            if (position + 1 < _jobConfirmations.Count)
            {
                toRequeue = orderedList.GetRange(index: position + 1, 
                                                 count: _jobConfirmations.Count() - position - 1).ToHashSet();
            }

            return toRequeue;
        }

        public HashSet<FJobConfirmation> CutTailByStack(long currentTime, FJobConfirmation jobConfirmation)
        {
            var allPreviousJobs = _jobConfirmations.Where(e => e.Job.Priority(currentTime) <= jobConfirmation.Job.Priority(currentTime)).ToList();
            var resourceTools = allPreviousJobs.Select(x => x.Job.RequiredCapability.Id).Distinct().ToList();

            var toRequeue = _jobConfirmations.Where(e => e.Job.Priority(currentTime) > jobConfirmation.Job.Priority(currentTime)
                                                        && (!resourceTools.Contains(e.Job.RequiredCapability.Id) 
                                                        || e.Job.RequiredCapability.Id == jobConfirmation.Job.RequiredCapability.Id))
                                                            .ToHashSet();


            return toRequeue;
        }

        internal bool RemoveJob(FJobConfirmation jobConfirmation)
        {
            return _jobConfirmations.Remove(item: jobConfirmation);
        }

        internal bool UpdatePreCondition(FUpdateStartCondition startCondition)
        {
            var jobConfirmation = _jobConfirmations.SingleOrDefault(x => x.Job.Key == startCondition.OperationKey);
            if (jobConfirmation == null) return false;
            jobConfirmation.Job.StartConditions.ArticlesProvided = startCondition.ArticlesProvided;
            jobConfirmation.Job.StartConditions.PreCondition = startCondition.PreCondition;
            return jobConfirmation.Job.StartConditions.ArticlesProvided && jobConfirmation.Job.StartConditions.PreCondition;
        }
    }
}
