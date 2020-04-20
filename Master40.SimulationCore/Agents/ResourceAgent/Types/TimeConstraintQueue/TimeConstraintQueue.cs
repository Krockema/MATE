using Akka.Actor;
using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBuckets;
using static FJobConfirmations;
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
            var bucket = GetFirstSatisfied(currentTime);
            this.Remove(bucket.Key);
            return bucket.Value;
        }

        public void Enqueue(IConfirmation jobConfirmation)
        {
            foreach(var scope in jobConfirmation.ScopeConfirmation.Scopes)
            {
                this.Add(scope.Start, jobConfirmation);
            }
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

        public IConfirmation GetConfirmation(Guid jobKey)
        {
            return this.Values.Single(x => x.Job.Key == jobKey);
        }

        public bool RemoveJob(IConfirmation job)
        {
            return this.Remove(job.ScopeConfirmation.GetScopeStart());
        }

        public HashSet<IConfirmation> GetTail(long currentTime, IConfirmation jobConfirmation)
        {
            // queued before another item?
            var toRequeue = this.Where(x => x.Key >= jobConfirmation.ScopeConfirmation.GetScopeStart()
                                           && x.Value.Job.Priority(currentTime) >= jobConfirmation.Job.Priority(currentTime))
                .Select(x => x.Value).ToHashSet();
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

        public List<FQueueingScope> GetQueueAbleTime(FRequestProposalForCapabilityProvider jobProposal
                                , long currentTime, CapabilityProviderManager cpm, long resourceBlockedUntil, IActorRef resourceRef)
        {
            // 1. Für welche Prozessschritte werde ich gebraucht
            // 2. Errechne meine benötigte Dauer
            // 3. Gib mögliche Zeiträume zurück

            // 1. Zu Erstens
            var capabilityProvider = cpm.GetCapabilityProviderByCapability(jobProposal.CapabilityProviderId);
            var setup = capabilityProvider.ResourceSetups.Single(x => resourceRef.Equals(x.Resource.IResourceRef));

            // 1 Methode für Resource setup, Processing

            // Methode für Resource Setup + Processing
            var requiredDuration = 0L;
            if (setup.UsedInProcess)
                requiredDuration += ((FBucket)jobProposal.Job).MaxBucketSize;

            if (setup.UsedInSetup)
                requiredDuration += setup.SetupTime;

            var usedForSetupAndProcess = setup.UsedInProcess && setup.UsedInSetup;

            var queuingScopes = GetScopesFor(requiredTime: requiredDuration, 
                                                jobProposal.Job, 
                                                jobProposal.CapabilityProviderId,
                                                usedForSetupAndProcess, 
                                                currentTime, 
                                                resourceBlockedUntil, 
                                                cpm);
            return queuingScopes;
        }

        private List<FQueueingScope> GetScopesFor(long requiredTime, IJob job, int capabilityProviderId, bool usedForSetupAndProcess,  long currentTime, long totalWorkLoad, CapabilityProviderManager capabilityProviderManager)
        {
            var scopes = new List<ITimeRange>();
            var positions = new List<FQueueingScope>();

            var jobPriority = job.Priority(currentTime);
            var allWithLowerPriority = this.Where(x => x.Value.Job.Priority(currentTime) <= jobPriority);
            var enumerator = allWithLowerPriority.GetEnumerator();
            var isQueueAble = false;
            var isRequiringSetup = true;
            if (!enumerator.MoveNext())
            {
                isQueueAble = true;
                isRequiringSetup = capabilityProviderManager.AlreadyEquipped(capabilityProviderId);
                // ignore first round
                // totalwork bis max
            }
            else
            {
                var current = enumerator.Current;
                var preScopeEnd = current.Value.ScopeConfirmation.GetScopeEnd();
                totalWorkLoad = preScopeEnd;
                while (enumerator.MoveNext())
                {
                    // Limit? Job satisfied?
                    isQueueAble = Limit > totalWorkLoad || ((FBucket)job).StartConditions.Satisfied;
                    if(!isQueueAble)           
                        break;

                    var post = enumerator.Current;
                    var postScopeStart = post.Value.ScopeConfirmation.GetScopeStart();
                    var setupTimeIfEquipped = 0L;
                    if (usedForSetupAndProcess)
                    {
                        setupTimeIfEquipped = GetSetupTimeIfEquipped(capabilityProviderManager, capabilityProviderId);
                        requiredTime -= setupTimeIfEquipped;
                    }

                    if (requiredTime <= postScopeStart - preScopeEnd)
                    {
                        positions.Add(new FQueueingScope(isQueueAble: true,
                                                        isRequieringSetup: (setupTimeIfEquipped == 0), // setup is Required
                                                        scope: new FScope(start: totalWorkLoad, end: postScopeStart)
                                                        ));

                    }
                    current = post;
                    totalWorkLoad = current.Value.ScopeConfirmation.GetScopeEnd();
                }
            }

            //
            enumerator.Dispose();
            // Queue contains no job --> Add queable item
            positions.Add(new FQueueingScope(isQueueAble: isQueueAble,
                                                isRequieringSetup: isRequiringSetup,
                                                scope: new FScope(start: totalWorkLoad, end: long.MaxValue)
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

        private long GetRequiredSetupTime(CapabilityProviderManager cpm, int currentId, FRequestProposalForCapabilityProvider requestProposalForCapabilityProvider)
        {
            if (currentId == requestProposalForCapabilityProvider.Job.RequiredCapability.Id) return 0L;
            return cpm.GetSetupDurationByCapabilityProvider(requestProposalForCapabilityProvider.CapabilityProviderId);
        }

        public IConfirmation FirstOrNull()
        {
            return this.Count > 0 ? this.Values.First() : null;
        }

        public bool CheckScope(IConfirmation fJobConfirmation, long time)
        {
            var jobPriority = fJobConfirmation.Job.Priority(time);
            var allWithLowerPriority = this.Where(x => x.Value.Job.Priority(time) <= jobPriority);
            var scopeConfirmation = fJobConfirmation.ScopeConfirmation;
            var fitEnd = true;
            var fitStart = true;
            var pre = allWithLowerPriority.OrderByDescending(x => x.Key)
                                                                      .FirstOrDefault(x => x.Key <= scopeConfirmation.GetScopeStart());
            var post = allWithLowerPriority.FirstOrDefault(x => x.Key > scopeConfirmation.GetScopeStart());

            if (pre.Value == null)
                return true;

            fitStart = pre.Value.ScopeConfirmation.GetScopeEnd() <= scopeConfirmation.GetScopeStart();
            if (post.Value != null)
                  fitEnd = post.Key > scopeConfirmation.GetScopeEnd();

            return fitStart && fitEnd;
        }

        public long Workload => this.Values.Sum(x => x.Job.Duration);
    }
}
