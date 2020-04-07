using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using static FBuckets;
using static FJobConfirmations;
using static FRequestProposalForSetups;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue { 
    public class TimeConstraintQueue : SortedList<long, FJobConfirmation>, IJobQueue
    {
        public int Limit { get; set; }

        public TimeConstraintQueue(int limit)
        {
            Limit = limit;
        }

        public FJobConfirmation DequeueFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability = null)
        {
            var bucket = GetFirstSatisfied(currentTime);
            this.Remove(bucket.Key);
            return bucket.Value;
        }

        public void Enqueue(FJobConfirmation jobConfirmation)
        {
            this.Add(jobConfirmation.Schedule, jobConfirmation);
        }

        public KeyValuePair<long, FJobConfirmation> GetFirstSatisfied(long currentTime)
        {
            var bucket = this.First(x => ((FBucket)x.Value.Job).HasSatisfiedJob);
            return bucket;
        }

        public bool CapacitiesLeft()
        {
            return Limit > GetJobsAs<FBucket>().Sum(selector: x => x.Scope);
        }

        public bool HasQueueAbleJobs()
        {
            var hasSatisfiedJob = false;
            foreach (var job in this.Values)
            {
                if (((FBucket)job.Job).HasSatisfiedJob)
                {
                    hasSatisfiedJob = true;
                }
            }
            return hasSatisfiedJob;
        }

        public IEnumerable<T> GetJobsAs<T>()
        {
            return this.Values.Cast<T>();
        }

        public T GetJobAs<T>(Guid key)
        {
            throw new NotImplementedException();
        }

        public FJobConfirmation GetConfirmation(Guid key)
        {
            return this.Values.Single(x => x.Job.Key == key);
        }

        public bool RemoveJob(FJobConfirmation job)
        {
            return this.Remove(job.Schedule);
        }

        public HashSet<FJobConfirmation> CutTail(long currentTime, FJobConfirmation jobConfirmation)
        {
            // queued before another item?
            var toRequeue = this.Where(x => x.Key > jobConfirmation.Schedule
                                           && x.Value.Job.Priority(currentTime) >= jobConfirmation.Job.Priority(currentTime))
                .Select(x => x.Value).ToHashSet();
            return toRequeue;
        }

        public List<QueueingPosition> GetQueueAbleTime(FRequestProposalForSetup jobProposal, long currentTime, long processingQueueLength, long resourceIsBlockedUntil, int currentSetupId = -1)
        {
            
            List<QueueingPosition> positions = new List<QueueingPosition>();
            var job = jobProposal.Job;
            var totalWorkLoad = 0L;
            var jobPriority = jobProposal.Job.Priority(currentTime);
            var allWithLowerPriority = this.Where(x => x.Value.Job.Priority(currentTime) <= jobPriority);
            var enumerator = allWithLowerPriority.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                // Queue contains no job --> Add queable item.
                positions.Add(new QueueingPosition(isQueueAble: true,
                                                    estimatedStart: currentTime + GetRequiredSetupTime(currentSetupId, jobProposal.SetupId)));
            }
            else
            {
                var current = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    var endPre = current.Key + current.Value.Schedule;
                    var startPost = enumerator.Current.Key;
                    var requiredSetupTime = GetRequiredSetupTime(current.Value.SetupDefinition.SetupKey, jobProposal.SetupId);
                    if (endPre <= startPost - job.Duration - requiredSetupTime)
                    {
                        // slotFound = validSlots.TryAdd(endPre, startPost - endPre);
                        positions.Add(new QueueingPosition(isQueueAble: true,
                                                            estimatedStart: endPre));
                    }

                    totalWorkLoad = endPre + ((FBucket) current.Value.Job).Scope + requiredSetupTime;
                    current = enumerator.Current;
                }

                if ((totalWorkLoad < Limit || ((FBucket) job).HasSatisfiedJob)) // TODO: Maybe not required. 
                {
                    positions.Add(new QueueingPosition(isQueueAble: true,
                                                        estimatedStart: currentTime + totalWorkLoad));
                }
            }
            enumerator.Dispose();
            return positions;
        }

        private long GetRequiredSetupTime(int currentSetup, int requiredSetupId)
        {
            return 0L; // TODO
        }

        public FJobConfirmation FirstOrNull()
        {
            return this.Count > 0 ? this.Values.First() : null;
        }
    }
}
