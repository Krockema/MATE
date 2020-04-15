using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBuckets;
using static FJobConfirmations;
using static FQueueingScopes;
using static FRequestProposalForCapabilityProviders;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue
{
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
            this.Add(jobConfirmation.ScopeConfirmation.Start, jobConfirmation);
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
            return this.Values.Select(x => x.Job).Cast<T>();
        }

        public T GetJobAs<T>(Guid key)
        {
            var jobConfirmation = this.SingleOrDefault(x => x.Value.Job.Key == key).Value;
            return (T)Convert.ChangeType(jobConfirmation.Job, typeof(T));
        }

        public FJobConfirmation GetConfirmation(Guid key)
        {
            return this.Values.SingleOrDefault(x => x.Job.Key == key);
        }

        public bool RemoveJob(FJobConfirmation job)
        {
            return this.Remove(job.ScopeConfirmation.Start);
        }

        public HashSet<FJobConfirmation> CutTail(long currentTime, FJobConfirmation jobConfirmation)
        {
            // queued before another item?
            var toRequeue = this.Where(x => x.Key >= jobConfirmation.ScopeConfirmation.Start
                                           && x.Value.Job.Priority(currentTime) >= jobConfirmation.Job.Priority(currentTime))
                .Select(x => x.Value).ToHashSet();
            return toRequeue;
        }

        public List<FQueueingScope> GetQueueAbleTime(FRequestProposalForCapabilityProvider jobProposal
                                , long currentTime, CapabilityProviderManager cpm)
        {
            
            var positions = new List<FQueueingScope>();
            var job = jobProposal.Job;
            var jobPriority = jobProposal.Job.Priority(currentTime);
            var allWithLowerPriority = this.Where(x => x.Value.Job.Priority(currentTime) <= jobPriority);
            var enumerator = allWithLowerPriority.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                var requiredSetupTime = GetRequiredSetupTime(cpm, jobProposal.CapabilityProviderId);
                // Queue contains no job --> Add queable item.
                positions.Add(new FQueueingScope(isQueueAble: true,
                                                    isRequieringSetup: (requiredSetupTime>0)?true:false,
                                                    start: currentTime,
                                                    end: long.MaxValue,
                                                    estimatedSetup: requiredSetupTime,
                                                    estimatedWork: ((FBucket)jobProposal.Job).MaxBucketSize
                                                    ));
            }
            else
            {
                var current = enumerator.Current;
                var totalWorkLoad = current.Value.ScopeConfirmation.End;
                long requiredSetupTime = 0;
                while (enumerator.MoveNext())
                {
                    var endPre = current.Key + ((FBucket) current.Value.Job).MaxBucketSize;
                    var startPost = enumerator.Current.Key;
                    requiredSetupTime = GetRequiredSetupTime(cpm, current.Value.CapabilityProvider.ResourceCapabilityId, jobProposal);
                    if (endPre <= startPost - ((FBucket)jobProposal.Job).MaxBucketSize - requiredSetupTime)
                    {
                        // slotFound = validSlots.TryAdd(endPre, startPost - endPre);
                        positions.Add(new FQueueingScope(isQueueAble: true,
                                                    isRequieringSetup: (requiredSetupTime > 0) ? true : false,
                                                    start: endPre,
                                                    end: startPost,
                                                    estimatedSetup: requiredSetupTime,
                                                    estimatedWork: ((FBucket)jobProposal.Job).MaxBucketSize
                                                    ));
                    }


                    // totalWorkLoad = endPre + ((FBucket) current.Value.Job).MaxBucketSize + requiredSetupTime;
                    current = enumerator.Current;
                    totalWorkLoad = current.Value.ScopeConfirmation.End;
                    
                }

                positions.Add(new FQueueingScope(isQueueAble: (totalWorkLoad < Limit || ((FBucket)job).HasSatisfiedJob),
                                                    isRequieringSetup: (requiredSetupTime > 0) ? true : false,
                                                    start: totalWorkLoad,
                                                    end: long.MaxValue,
                                                    estimatedSetup: requiredSetupTime,
                                                    estimatedWork: ((FBucket)jobProposal.Job).MaxBucketSize
                                                    )) ;

            }
            enumerator.Dispose();
            return positions;
        }

        private long GetRequiredSetupTime(CapabilityProviderManager cpm, int capabilityProviderId)
        {
            if (cpm.AlreadyEquipped(capabilityProviderId)) return 0L;
            return cpm.GetSetupDurationByCapabilityProvider(capabilityProviderId);
        }

        private long GetRequiredSetupTime(CapabilityProviderManager cpm, int currentId, FRequestProposalForCapabilityProvider requestProposalForCapabilityProvider)
        {
            if (currentId == requestProposalForCapabilityProvider.Job.RequiredCapability.Id) return 0L;
            return cpm.GetSetupDurationByCapabilityProvider(requestProposalForCapabilityProvider.CapabilityProviderId);
        }

        public FJobConfirmation FirstOrNull()
        {
            return this.Count > 0 ? this.Values.First() : null;
        }

        public bool CheckScope(FJobConfirmation fJobConfirmation, long time)
        {
            var jobPriority = fJobConfirmation.Job.Priority(time);
            var allWithLowerPriority = this.Where(x => x.Value.Job.Priority(time) <= jobPriority);
            var scopeConfirmation = fJobConfirmation.ScopeConfirmation;
            var fitEnd = true;
            var fitStart = true;
            var pre = allWithLowerPriority.OrderByDescending(x => x.Key)
                                                                      .FirstOrDefault(x => x.Key <= scopeConfirmation.Start);
            var post = allWithLowerPriority.FirstOrDefault(x => x.Key > scopeConfirmation.Start);

            if (pre.Value == null)
                return true;

            fitStart = pre.Value.ScopeConfirmation.End <= scopeConfirmation.Start;
            if (post.Value != null)
                  fitEnd = post.Key > scopeConfirmation.End;

            return fitStart && fitEnd;
        }

        public long Workload => this.Values.Sum(x => x.Job.Duration);
    }
}
