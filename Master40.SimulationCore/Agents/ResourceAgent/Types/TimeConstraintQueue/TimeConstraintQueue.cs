using Akka.Actor;
using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using NLog.LayoutRenderers;
using static FBuckets;
using static FQueueingScopes;
using static FRequestProposalForCapabilityProviders;
using static FScopes;
using static IConfirmations;
using static IJobs;
using static ITimeRanges;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue
{
    public class TimeConstraintQueue : SortedList<long, IConfirmation>, IJobQueue
    {
        public int Limit { get; set; }
        
        public TimeConstraintQueue(int limit)
        {
            Limit = limit;
        }

        public IConfirmation DequeueFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability = null)
        {
            var (key, value) = GetFirstSatisfied(currentTime);
            this.Remove(key);
            return value;
        }

        public IConfirmation GetFirstIfSatisfied(long currentTime, M_ResourceCapability resourceCapability = null)
        {
            var (key, confirmation) = GetFirst();
            if (confirmation != null && ((FBucket)confirmation.Job).HasSatisfiedJob)
            {
                
                return confirmation;
            }
            return null;
        }
        public KeyValuePair<long, IConfirmation> GetFirst()
        {
            var bucket = this.FirstOrDefault();
            return bucket;
        }
        public void Enqueue(IConfirmation jobConfirmation)
        {
            this.Add(jobConfirmation.ScopeConfirmation.GetScopeStart(), jobConfirmation);
        }

        public KeyValuePair<long, IConfirmation> GetFirstSatisfied(long currentTime)
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
            return this.Values.Any(job => ((FBucket) job.Job).HasSatisfiedJob);
        }

        public bool FirstJobIsQueueAble()
        {
            return (this.Values.Count != 0) && ((FBucket)this.Values[0].Job).HasSatisfiedJob;
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

        public IConfirmation GetConfirmation(Guid jobKey)
        {
            return this.Values.SingleOrDefault(x => x.Job.Key == jobKey);
        }

        public bool RemoveJob(IConfirmation job)
        {
            return this.Remove(job.ScopeConfirmation.GetScopeStart());
        }

        public HashSet<IConfirmation> GetTail(long currentTime, IConfirmation jobConfirmation)
        {

            //   
            var toRequeue2 = 
                this.Where(x => x.Key <= jobConfirmation.ScopeConfirmation.GetScopeStart()
                                                      && x.Value.ScopeConfirmation.GetScopeEnd() > jobConfirmation.ScopeConfirmation.GetScopeStart())
                                                .Select(x => x.Value).ToHashSet();

            // queued before another item?
            var toRequeue = this.Where(x => (x.Key >= jobConfirmation.ScopeConfirmation.GetScopeStart()
                                                      && x.Value.Job.Priority(currentTime) >= jobConfirmation.Job.Priority(currentTime))
                                                     // || (x.Value.ScopeConfirmation.GetScopeEnd() >= jobConfirmation.&& x.Value.Job.Priority(currentTime) >= jobConfirmation.Job.Priority(currentTime)
                                                      )
                                                  .Select(x => x.Value).ToHashSet();
            toRequeue.UnionWith(toRequeue2);
            return toRequeue;
        }

        public HashSet<IConfirmation> GetAllSubsequentJobs(long jobStart)
        {
            return this.Where(x => x.Key >= jobStart).Select(x => x.Value).ToHashSet();
        }

        public HashSet<IConfirmation> GetAllJobs()
        {
            return this.Select(x => x.Value).ToHashSet();
        }

        public List<FQueueingScope> GetQueueAbleTime(FRequestProposalForCapability jobProposal
                                , long currentTime, CapabilityProviderManager cpm, long resourceBlockedUntil, IActorRef resourceRef)
        {
            // 1. Für welche Prozessschritte werde ich gebraucht
            // 2. Errechne meine benötigte Dauer
            // 3. Gib mögliche Zeiträume zurück

            // 1. Zu Erstens
            var capabilityProvider = cpm.GetCapabilityProviderByCapability(jobProposal.CapabilityId);
            var setup = capabilityProvider.ResourceSetups.Single(x => resourceRef.Equals(x.Resource.IResourceRef));

            // 1 Methode für Resource setup, Processing

            // Methode für Resource Setup + Processing
            var requiredDuration = 0L;
            if (setup.UsedInProcess)
                requiredDuration += ((FBucket)jobProposal.Job).MaxBucketSize;

            if (setup.UsedInSetup)
                requiredDuration += setup.SetupTime;

            var usedForSetupAndProcess = setup.UsedInProcess && setup.UsedInSetup;

            var queuingScopes = GetScopesFor(requiredTimeforJob: requiredDuration, 
                                                jobProposal.Job, 
                                                jobProposal.CapabilityId,
                                                usedForSetupAndProcess, 
                                                currentTime, 
                                                resourceBlockedUntil, 
                                                cpm);
            return queuingScopes;
        }

        private List<FQueueingScope> GetScopesFor(long requiredTimeforJob, IJob job, int capabilityProviderId, bool usedForSetupAndProcess,  long currentTime, long resourceBlockedUntil, CapabilityProviderManager capabilityProviderManager)
        {
            var scopes = new List<ITimeRange>();
            var positions = new List<FQueueingScope>();

            var jobPriority = job.Priority(currentTime);
            var allWithHigherPriority = this.Where(x => x.Value.Job.Priority(currentTime) <= jobPriority);
            var enumerator = allWithHigherPriority.GetEnumerator();
            var isQueueAble = false;
            var isRequiringSetup = true;
            var earliestStart = currentTime + resourceBlockedUntil;
            if (!enumerator.MoveNext())
            {
                isQueueAble = true;
                isRequiringSetup = (!capabilityProviderManager.AlreadyEquipped(capabilityProviderId));
                // ignore first round
                // totalwork bis max
            }
            else
            {
                var current = enumerator.Current;
                var preScopeEnd = current.Value.ScopeConfirmation.GetScopeEnd();
                var preIsReady = ((FBucket) current.Value.Job).HasSatisfiedJob;
                var jobToPutIsReady = ((FBucket) job).HasSatisfiedJob;
                var currentJobPriority = current.Value.Job.Priority(currentTime);
                isRequiringSetup = (!capabilityProviderManager.AlreadyEquipped(capabilityProviderId));
                isQueueAble = currentTime + Limit > earliestStart + ((FBucket)job).MaxBucketSize || ((FBucket)job).HasSatisfiedJob;
                
                
                earliestStart = !preIsReady && jobToPutIsReady && (currentJobPriority == jobPriority) ? earliestStart : preScopeEnd;
                //earliestStart = preScopeEnd;
                while (enumerator.MoveNext())
                {
                    // Limit? Job satisfied?
                    if (!isQueueAble)
                        break;

                    var requiredTime = requiredTimeforJob;
                    var post = enumerator.Current;
                    var postScopeStart = post.Value.ScopeConfirmation.GetScopeStart();
                    var setupTimeIfEquipped = 0L;
                    if (usedForSetupAndProcess)
                    {
                        setupTimeIfEquipped = GetSetupTimeIfEquipped(capabilityProviderManager, capabilityProviderId);
                        requiredTime -= setupTimeIfEquipped;
                    }

                    if (requiredTime <= postScopeStart - earliestStart)
                    {
                        positions.Add(new FQueueingScope(isQueueAble: true,
                                                        isRequieringSetup: (setupTimeIfEquipped == 0), // setup is Required
                                                        scope: new FScope(start: earliestStart, end: postScopeStart)
                                                        ));

                    }
                    current = post;
                    currentJobPriority = current.Value.Job.Priority(currentTime);
                    preIsReady = ((FBucket) current.Value.Job).HasSatisfiedJob;
                    //earliestStart = current.Value.ScopeConfirmation.GetScopeEnd();
                    earliestStart = !preIsReady && jobToPutIsReady 
                                    && currentJobPriority == jobPriority
                                    ? earliestStart : current.Value.ScopeConfirmation.GetScopeEnd();
                    isQueueAble = currentTime + Limit > earliestStart + ((FBucket)job).MaxBucketSize || ((FBucket)job).HasSatisfiedJob;
                }
            }

            //
            enumerator.Dispose();
            // Queue contains no job --> Add queable item
            positions.Add(new FQueueingScope(isQueueAble: isQueueAble,
                                                isRequieringSetup: isRequiringSetup,
                                                scope: new FScope(start: earliestStart, end: long.MaxValue)
                                                ));
            return positions;
        }




        private long GetRequiredSetupTime(CapabilityProviderManager cpm, int capabilityProviderId)
        {
            if (cpm.AlreadyEquipped(capabilityProviderId)) return 0L;
            return cpm.GetSetupDurationByCapabilityProvider(capabilityProviderId);
        }

        private long GetSetupTimeIfEquipped(CapabilityProviderManager cpm, int capabilityProviderId)
        {
            if (cpm.AlreadyEquipped(capabilityProviderId)) return cpm.GetSetupDurationByCapabilityProvider(capabilityProviderId);
            return 0L;
        }

        private long GetRequiredSetupTime(CapabilityProviderManager cpm, int currentId, FRequestProposalForCapability requestProposalForCapabilityProvider)
        {
            if (currentId == requestProposalForCapabilityProvider.Job.RequiredCapability.Id) return 0L;
            return cpm.GetSetupDurationByCapabilityProvider(requestProposalForCapabilityProvider.CapabilityId);
        }

        public IConfirmation FirstOrNull()
        {
            return this.Count > 0 ? this.Values.First() : null;
        }

        public bool CheckScope(IConfirmation confirmation, long time)
        {
            var priority = confirmation.Job.Priority(time);
            var allWithHigherPriority = this.Where(x => x.Value.Job.Priority(time) <= priority).ToList();
            var fitEnd = true;
            var fitStart = true;

            var pre = allWithHigherPriority.OrderByDescending(x => x.Key)
                                                                    .FirstOrDefault(x => x.Key <= confirmation.ScopeConfirmation.GetScopeStart());
            var post = allWithHigherPriority.FirstOrDefault(x => x.Key > confirmation.ScopeConfirmation.GetScopeEnd());

            

            if (pre.Value == null)
                return true;

            var preReady = ((FBucket) pre.Value.Job).HasSatisfiedJob;
            var jobToPutReady = ((FBucket)confirmation.Job).HasSatisfiedJob;
            fitStart = pre.Value.ScopeConfirmation.GetScopeEnd() <= confirmation.ScopeConfirmation.GetScopeStart();
            //fitStart = readyCondition || pre.Value.ScopeConfirmation.GetScopeEnd() <= confirmation.ScopeConfirmation.GetScopeStart();
            if (pre.Value.Job.Priority(time) == priority && !preReady && jobToPutReady)
                fitStart = true;


            
            //fitStart = pre.Value.ScopeConfirmation.GetScopeEnd() <= confirmation.ScopeConfirmation.GetScopeStart();
            if (post.Value != null)
                  fitEnd = post.Key > confirmation.ScopeConfirmation.GetScopeEnd();

            return fitStart && fitEnd;
        }

        public bool UpdateBucket(IJob job)
        {
            var result = this.Single(x => x.Value.Job.Key.Equals(job.Key));
            var returnVal = this.Remove(result.Key);
            var newJobConfirmation = result.Value.UpdateJob(job);
            this.Add(result.Key, newJobConfirmation);
            return returnVal;
        }

        public long Workload => this.Values.Sum(x => x.Job.Duration);
    }
}
