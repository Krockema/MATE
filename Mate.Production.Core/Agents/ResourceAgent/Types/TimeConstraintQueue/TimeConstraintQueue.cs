using Mate.DataCore.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Environment.Records.Scopes;

namespace Mate.Production.Core.Agents.ResourceAgent.Types.TimeConstraintQueue
{
    public class TimeConstraintQueue : SortedList<DateTime, IConfirmation>, IJobQueue
    {
        public TimeSpan Limit { get; set; }
        public TimeSpan Workload => this.Values.Sum(x => x.Job.Duration);

        public TimeConstraintQueue(TimeSpan limit)
        {
            Limit = limit;
        }

        public IConfirmation GetFirstIfSatisfied(DateTime currentTime, M_ResourceCapability resourceCapability = null)
        {
            var (key, confirmation) = GetFirst();
            if (confirmation != null && ((BucketRecord)confirmation.Job).HasSatisfiedJob())
            {
                return confirmation;
            }
            return null;
        }

        public IConfirmation DequeueFirstSatisfied(DateTime currentTime, M_ResourceCapability resourceCapability = null)
        {
            var (key, value) = GetFirstSatisfied(currentTime);
            this.Remove(key);
            return value;
        }

        public IConfirmation GetFirstIfSatisfiedAndSetReadyAtIsSmallerOrEqualThan(DateTime currentTime, M_ResourceCapability resourceCapability = null)
        {
            var (key, confirmation) = GetFirst();
            if (confirmation != null 
                && ((BucketRecord)confirmation.Job).HasSatisfiedJob() 
                && confirmation.ScopeConfirmation.SetReadyAt <= currentTime)
            {
                return confirmation;
            }
            return null;
        }
        public KeyValuePair<DateTime, IConfirmation> GetFirst()
        {
            var bucket = this.FirstOrDefault();
            return bucket;
        }
        public void Enqueue(IConfirmation jobConfirmation)
        {
            this.Add(jobConfirmation.ScopeConfirmation.GetScopeStart(), jobConfirmation);
        }

        public KeyValuePair<DateTime, IConfirmation> GetFirstSatisfied(DateTime currentTime)
        {
            var bucket = this.First(x => ((BucketRecord)x.Value.Job).HasSatisfiedJob());
            return bucket;
        }

        public bool CapacitiesLeft()
        {
            return Limit > GetJobsAs<BucketRecord>().Sum(x => x.Scope);
        }

        public bool HasQueueAbleJobs()
        {
            return this.Values.Any(job => ((BucketRecord)job.Job).HasSatisfiedJob());
        }

        public bool FirstJobIsQueueAble()
        {
            return (this.Values.Count != 0) && ((BucketRecord)this.Values[0].Job).HasSatisfiedJob();
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

        public HashSet<IConfirmation> GetTail(DateTime currentTime, IConfirmation jobConfirmation)
        {

            var toRequeue = this.Where(x => (x.Value.ScopeConfirmation.GetScopeStart() >= jobConfirmation.ScopeConfirmation.GetScopeStart() 
                                             && x.Value.ScopeConfirmation.GetScopeStart() < jobConfirmation.ScopeConfirmation.GetScopeEnd()) 
                                            || (x.Value.ScopeConfirmation.GetScopeEnd() > jobConfirmation.ScopeConfirmation.GetScopeStart()
                                             && x.Value.ScopeConfirmation.GetScopeEnd() <= jobConfirmation.ScopeConfirmation.GetScopeEnd())
                                            || (x.Value.ScopeConfirmation.GetScopeStart() < jobConfirmation.ScopeConfirmation.GetScopeEnd() 
                                             && jobConfirmation.ScopeConfirmation.GetScopeStart() < x.Value.ScopeConfirmation.GetScopeEnd()                                                )
                                            && x.Value.Job.Priority(currentTime) >= jobConfirmation.Job.Priority(currentTime))
                                            .Select(x => x.Value).ToHashSet();

            return toRequeue;
        }
        


        public HashSet<IConfirmation> GetAllSubsequentJobs(DateTime jobStart)
        {
            return this.Where(x => x.Key >= jobStart).Select(x => x.Value).ToHashSet();
        }

        public HashSet<IConfirmation> GetAllJobs()
        {
            return this.Select(x => x.Value).ToHashSet();
        }

        public List<QueueingScopeRecord> GetQueueAbleTime(RequestProposalForCapabilityRecord jobProposal
                                , DateTime currentTime, CapabilityProviderManager cpm
                                , DateTime resourceBlockedUntil, int resourceId)
        {

            //TODO Right now only take the first
            var resourceCapabilityProvider = cpm.GetCapabilityProviderByCapability(jobProposal.CapabilityId);
            var setup = resourceCapabilityProvider.ResourceSetups.Single(x => resourceId.Equals(x.Resource.Id));

            var requiredDuration = TimeSpan.Zero;
            if (setup.UsedInProcess)
                requiredDuration += ((BucketRecord)jobProposal.Job).MaxBucketSize;

            if (setup.UsedInSetup)
                requiredDuration += resourceCapabilityProvider.ResourceSetups.Sum(x => x.SetupTime);

            var usedForSetupAndProcess = setup.UsedInProcess && setup.UsedInSetup;

            var queuingScopes = GetScopesFor(requiredTimeForJob: requiredDuration, 
                                                jobProposal.Job, 
                                                //jobProposal.CapabilityId,
                                                resourceCapabilityProvider.ResourceCapabilityId,
                                                usedForSetupAndProcess, 
                                                currentTime, 
                                                resourceBlockedUntil, 
                                                cpm);
            return queuingScopes;
        }

        private List<QueueingScopeRecord> GetScopesFor(TimeSpan requiredTimeForJob
                                                , IJob job
                                                //, int capabilityId
                                                , int resourceCapabilityId
                                                , bool usedForSetupAndProcess
                                                , DateTime currentTime
                                                , DateTime resourceBlockedUntil
                                                , CapabilityProviderManager capabilityProviderManager)
        {
            var positions = new List<QueueingScopeRecord>();

            var jobPriority = job.Priority(currentTime);
            var allWithHigherPriority = this.Where(x => x.Value.Job.Priority(currentTime) <= jobPriority);
            var enumerator = allWithHigherPriority.GetEnumerator();
            var isQueueAble = false;
            var isRequiringSetup = true;
            var earliestStart = resourceBlockedUntil < currentTime ? currentTime : resourceBlockedUntil;
            var requiredTime = requiredTimeForJob;
            if (!enumerator.MoveNext())
            {
                isQueueAble = true;
                isRequiringSetup = (!capabilityProviderManager.AlreadyEquipped(resourceCapabilityId));
                // ignore first round
                // totalwork = max
            }
            else
            {
                var current = enumerator.Current;
                var preScopeEnd = current.Value.ScopeConfirmation.GetScopeEnd();
                var preIsReady = ((BucketRecord) current.Value.Job).HasSatisfiedJob();
                var jobToPutIsReady = ((BucketRecord) job).HasSatisfiedJob();
                var currentJobPriority = current.Value.Job.Priority(currentTime);
                earliestStart = !preIsReady && jobToPutIsReady
                                    && (currentJobPriority > jobPriority) ? earliestStart : preScopeEnd;
                if (usedForSetupAndProcess)
                {

                    if (preScopeEnd == earliestStart)
                        isRequiringSetup = current.Value.CapabilityProvider.ResourceCapabilityId !=
                                           resourceCapabilityId;
                    else
                        isRequiringSetup = (!capabilityProviderManager.AlreadyEquipped(resourceCapabilityId));

                    if (!isRequiringSetup)
                        requiredTime = requiredTimeForJob - capabilityProviderManager.GetSetupDurationBy(resourceCapabilityId);
                }

                isQueueAble = currentTime + Limit > earliestStart + requiredTime || !preIsReady && jobToPutIsReady;

                //earliestStart = preScopeEnd;
                while (enumerator.MoveNext())
                {
                    // Limit? Job satisfied?
                    if (!isQueueAble) // earliestStart =  current.Value.ScopeConfirmation.GetScopeEnd();
                        break;

                    requiredTime = requiredTimeForJob;
                    var post = enumerator.Current;
                    var postScopeStart = post.Value.ScopeConfirmation.GetScopeStart();
                    
                    if (usedForSetupAndProcess)
                    {
                        isRequiringSetup = current.Value.CapabilityProvider.ResourceCapabilityId != resourceCapabilityId;
                        if (!isRequiringSetup)
                        {
                            requiredTime = requiredTimeForJob - capabilityProviderManager.GetSetupDurationBy(resourceCapabilityId);
                        }
                    }

                    if (requiredTime <= postScopeStart - earliestStart && requiredTime > TimeSpan.Zero)
                    {
                        positions.Add(new QueueingScopeRecord(IsQueueAble: true,
                                                        IsRequieringSetup: isRequiringSetup, // setup is Required
                                                        Scope: new ScopeRecord(Start: earliestStart, End: postScopeStart)
                                                        ));
                    }

                    current = post;
                    if (usedForSetupAndProcess)
                    {
                        isRequiringSetup = current.Value.CapabilityProvider.ResourceCapabilityId != resourceCapabilityId;
                        if (!isRequiringSetup)
                        {
                            requiredTime = requiredTimeForJob - capabilityProviderManager.GetSetupDurationBy(resourceCapabilityId);
                        }
                    }
                    currentJobPriority = current.Value.Job.Priority(currentTime);
                    preIsReady = ((BucketRecord) current.Value.Job).HasSatisfiedJob();
                    //earliestStart = current.Value.ScopeConfirmation.GetScopeEnd();


                    earliestStart = !preIsReady && jobToPutIsReady
                                    && (currentJobPriority > jobPriority)
                                    ? earliestStart : current.Value.ScopeConfirmation.GetScopeEnd();
                    isQueueAble = (currentTime + Limit) > (earliestStart + requiredTime) || jobToPutIsReady;
                    
                }
            }

            enumerator.Dispose();
            // Queue contains no job --> Add queable item
            // TODO only is queable if any position exits
            positions.Add(new QueueingScopeRecord(IsQueueAble: isQueueAble,
                                                IsRequieringSetup: isRequiringSetup,
                                                Scope: new ScopeRecord(Start: earliestStart, End: currentTime.AddYears(10))));
            return positions;
        }

        public IConfirmation FirstOrNull()
        {
            return this.Count > 0 ? this.Values.First() : null;
        }

        public bool CheckScope(IConfirmation confirmation, DateTime time, DateTime resourceIsBusyUntil, int equippedCapability, bool usedInSetup)
        {
            if (resourceIsBusyUntil > confirmation.ScopeConfirmation.GetScopeStart())
                return false;
            
            var hasSetup = confirmation.ScopeConfirmation.GetSetup();
            var priority = confirmation.Job.Priority(time);
            var allWithHigherPriority = this.Where(x => x.Value.Job.Priority(time) <= priority).ToList();
            var fitEnd = true;
            var fitStart = true;

            var pre = allWithHigherPriority.OrderByDescending(x => x.Key)
                                           .FirstOrDefault(x => x.Key <= confirmation.ScopeConfirmation.GetScopeStart());
            var post = allWithHigherPriority.FirstOrDefault(x => x.Key >= confirmation.ScopeConfirmation.GetScopeEnd());
            
            // check if setup is missing on "inProcessing" item
            if (pre.Value == null)
            {
                if (usedInSetup && confirmation.CapabilityProvider.ResourceCapabilityId.NotEqual(equippedCapability)
                    && hasSetup is null)
                {
                        return false;
                } 
                return true;
            }
            // check if setup is missing on previous item
            if (usedInSetup && pre.Value.CapabilityProvider.ResourceCapabilityId.NotEqual(confirmation.CapabilityProvider.ResourceCapabilityId) && hasSetup is null)
            {
                return false;
            }

            var preReady = ((BucketRecord)pre.Value.Job).HasSatisfiedJob();
            var jobToPutReady = ((BucketRecord)confirmation.Job).HasSatisfiedJob();
            fitStart = pre.Value.ScopeConfirmation.GetScopeEnd() <= confirmation.ScopeConfirmation.GetScopeStart();
            //fitStart = readyCondition || pre.Value.ScopeConfirmation.GetScopeEnd() <= confirmation.ScopeConfirmation.GetScopeStart();
            if (pre.Value.Job.Priority(time) >= priority && !preReady && jobToPutReady)
                fitStart = true;

            //fitStart = pre.Value.ScopeConfirmation.GetScopeEnd() <= confirmation.ScopeConfirmation.GetScopeStart();
            if (post.Value != null)
                  fitEnd = post.Key >= confirmation.ScopeConfirmation.GetScopeEnd();

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

        public bool QueueHealthCheck(DateTime currentTime)
        {
            if (this.Count <= 1) return false;

            var array = this.ToArray();                       //  21 > 200 --> false
            var inTime = array[0].Value.ScopeConfirmation.GetScopeStart() >= currentTime;
            if (inTime) return false;

            var priorityOfFirstElement = array[0].Value.Job.Priority(currentTime);
            for (var i = 1; i < array.Length; i++)
            {
                var satisfied = ((BucketRecord)array[i].Value.Job).HasSatisfiedJob();
                var priority = array[i].Value.Job.Priority(currentTime);
                if (satisfied && priority <= priorityOfFirstElement)
                {
                    return true;
                }
            }
            return false;
        }

        
        public int GetAmountOfPreviousOperations(DateTime scopeStart)
        {
            int operations = 0;
            //var index = this.IndexOfKey(scopeStart);

            foreach(var element in this.Where(x => x.Key < scopeStart))
            {
                operations += ((BucketRecord)element.Value.Job).Operations.Count();

            }

            return operations;
        }

        public (TimeSpan, int) GetPositionOfJobInJob(DateTime scopeStart, Guid operationId, DateTime currentTime)
        {
            int operations = 0;
            TimeSpan duration = TimeSpan.Zero;
            var bucket = ((BucketRecord)this[scopeStart].Job);
            var operation = bucket.Operations.Single(x => x.Key == operationId) as IJob;
            foreach (var element in bucket.Operations.Cast<IJob>().Where(x => x.Priority(currentTime) < operation.Priority(currentTime)))
            {
                operations++;
                duration += element.Duration;
            }

            return (duration, operations);
        }

    }
}

