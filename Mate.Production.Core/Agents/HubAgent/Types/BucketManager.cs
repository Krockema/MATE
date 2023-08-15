using System;
using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Agents.JobAgent;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Helper;
using NLog;

namespace Mate.Production.Core.Agents.HubAgent.Types
{
    /// <summary>
    /// hold all operations in buckets, ignoring if operation preconditions are satisfied or not
    /// </summary>
    public class BucketManager
    {
        private List<JobConfirmation>  _jobConfirmations { get; } = new List<JobConfirmation>();
        public Dictionary<int, BucketSize> CapabilityBucketSizeDictionary { get; } = new Dictionary<int, BucketSize>();

        private TimeSpan MaxBucketSize { get; set; }
        
        public BucketManager(TimeSpan maxBucketSize)
        {
            MaxBucketSize = maxBucketSize;
        }

        public JobConfirmation CreateBucket(OperationRecord fOperation, Agent agent)
        {

            var bucketSize = GetBucketSize(fOperation.RequiredCapability.Id);
            var bucket = fOperation.ToBucketScopeItem( agent.Context.Self, agent.CurrentTime, bucketSize);
            var jobConfirmation = new JobConfirmation(bucket);
            jobConfirmation.SetJobAgent(agent.Context.ActorOf(Job.Props(agent.ActorPaths, agent.HiveConfig, agent.Configuration, jobConfirmation.ToImmutable(), agent.Time, agent.DebugThis, agent.Context.Self)
                                       , $"JobAgent({bucket.Name.ToActorName()})"));
            _jobConfirmations.Add(jobConfirmation);
            return jobConfirmation;
        }

        public BucketRecord GetBucketByBucketKey(Guid bucketKey)
        {
            return _jobConfirmations.SingleOrDefault(x => x.Job.Key == bucketKey)?.Job as BucketRecord;
        }
        public JobConfirmation GetConfirmationByBucketKey(Guid bucketKey)
        {
            return _jobConfirmations.FirstOrDefault((x => x.Job.Key == bucketKey));
        }

        public void Replace(BucketRecord bucket)
        {
            _jobConfirmations.Single(x => x.Job.Key == bucket.Key).Job = bucket;
        }

        /// <summary>
        /// Find the operation in all buckets 
        /// </summary>
        /// <param name="operationKey"></param>
        /// <returns></returns>
        public OperationRecord GetOperationByOperationKey(Guid operationKey)
        {
            var bucket = GetBucketByOperationKey(operationKey);
            return bucket?.Operations.Single(x => x.Key == operationKey);
        }

        public BucketRecord GetBucketByOperationKey(Guid operationKey)
        {
            return _jobConfirmations.SingleOrDefault(x => ((BucketRecord)x.Job).Operations.Any(y => y.Key == operationKey))?.Job as BucketRecord;
        }

        public bool Remove(Guid bucketKey)
        {
            var jobConfirmation = GetConfirmationByBucketKey(bucketKey);
            var removed = _jobConfirmations.Remove(jobConfirmation);
            return removed;
        }

        public JobConfirmation GetAndRemoveJob(Guid bucketKey)
        {
            var jobConfirmation = GetConfirmationByBucketKey(bucketKey);
            if (jobConfirmation == null) return null;
            _jobConfirmations.Remove(jobConfirmation);
            return jobConfirmation;
        }

        public bool Remove(JobConfirmation jobConfirmation)
        {
            var removed = _jobConfirmations.Remove(jobConfirmation);
            return removed;
        }

        public BucketRecord Add(BucketRecord bucket, OperationRecord fOperation)
        {
            if (bucket == null) throw new Exception($"Bucket {bucket.Name} does not exits");
            bucket = bucket.AddOperation(fOperation);
            return bucket;
        }

        /// <summary>
        /// Add the operation to an existing matching bucket or otherwise creates a new one
        /// </summary>
        /// <param name="fOperation"></param>
        /// <param name="hubAgent"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public JobConfirmation AddToBucket(OperationRecord fOperation, DateTime time)
        {
            var matchingBuckets = FindAllWithEqualCapability(fOperation).Select(x => x.Job).Cast<BucketRecord>().ToList();

            if (matchingBuckets != null)
            {
                //Add
                var bucket = FindEarliestAndLatestScopeMatching(matchingBuckets, fOperation, time);
                if (bucket != null)
                {
                    bucket = Add(bucket: bucket, fOperation: fOperation);

                    var jobConfirmation = GetConfirmationByBucketKey(bucket.Key);
                    if (jobConfirmation == null) throw new Exception("Tried to add bucket to unknown jobConfirmation");
                    jobConfirmation.Job = bucket;
                    return jobConfirmation;
                }

            }
            //need to create a new one
            return null;
        }

        public IEnumerable<JobConfirmation> FindAllWithEqualCapability(OperationRecord fOperation)
        {
            return _jobConfirmations.Where(x => x.Job.RequiredCapability.Name == fOperation.RequiredCapability.Name);
        }

        public IEnumerable<JobConfirmation> FindAllWithEqualCapabilityThatAreNotFixed(OperationRecord fOperation)
        {
            return FindAllWithEqualCapability(fOperation).Where(x => !x.IsFixPlanned);
        }

        public List<JobConfirmation> FindAllBucketsLaterForwardStart(OperationRecord operation)
        {
            var matchingBuckets = FindAllWithEqualCapabilityThatAreNotFixed(operation);
            return matchingBuckets.Where(x => x.Job.ForwardStart > operation.ForwardStart).ToList();
        }

        /// <summary>
        /// Return null if no matching bucket exits
        /// </summary>
        /// <param name="buckets">gets a List of all bi</param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public BucketRecord FindEarliestAndLatestScopeMatching(List<BucketRecord> buckets, OperationRecord operation, DateTime time)
        {
            List<BucketRecord> matchingBuckets = new List<BucketRecord>();

            foreach (var bucket in buckets)
            {
                if (operation.RequiredCapability.IsBatchAble 
                    && bucket.Operations.Count < operation.RequiredCapability.BatchSize
                    && bucket.MaxBucketSize == operation.Operation.Duration
                    && HasLaterForwardStart(bucket, operation))
                {
                    matchingBuckets.Add(bucket);
                } 
                else 
                { 

                var maxbucket = ExceedMaxBucketSize(bucket, operation);
                if (HasCapacityLeft(bucket, operation) && HasLaterForwardStart(bucket, operation) && maxbucket)
                    matchingBuckets.Add(bucket);
                }
            }

            //var matchingBucket = GetBucketWithMostLeftCapacity(matchingBuckets);
            var matchingBucket = matchingBuckets.OrderBy(x => x.Priority(x, time)).FirstOrDefault();

            return matchingBucket;
        }

        private bool ExceedMaxBucketSize(BucketRecord bucket,OperationRecord operation)
        {
            return ((IJob) bucket).Duration + operation.Operation.Duration <= bucket.MaxBucketSize;
            //GetBucketSize(bucket.RequiredCapability.Id);
        }

        public BucketRecord GetBucketWithMostLeftCapacity(List<BucketRecord> buckets)
        {
            var bucket = buckets.Where(x => !x.IsFixPlanned).OrderBy(x => x.GetCapacityLeft()).FirstOrDefault();

            return bucket;
        }


        public bool HasCapacityLeft(BucketRecord bucket, OperationRecord operation)
        {
            return bucket.BackwardStart - bucket.ForwardStart > ((IJob)bucket).Duration + operation.Operation.Duration;
        }


        public bool HasLaterForwardStart(BucketRecord bucket, OperationRecord operation)
        {
            return operation.ForwardStart >= bucket.ForwardStart;
        }

        internal BucketRecord RemoveOperations(BucketRecord bucket, List<OperationRecord> operationsToRemove)
        {
            foreach (var operation in operationsToRemove)
            {
                DecreaseBucketSize(operation.RequiredCapability.Id, operation.Operation.Duration);
                bucket = bucket.RemoveOperation(operation);
            }

            return bucket;
        }

        public BucketRecord SetOperationStartCondition(Guid operationKey, UpdateStartConditionRecord startCondition, DateTime currentTime)
        {
            var bucket = GetBucketByOperationKey(operationKey);

            if (bucket != null)
            {
                var operation = bucket.Operations.Single(x => x.Key == operationKey);
                bucket.RemoveOperation(operation);

                operation.SetStartConditions(preCondition: startCondition.PreCondition, articleProvided: startCondition.ArticlesProvided, currentTime);
                operation = operation.UpdateCustomerDue(startCondition.CustomerDue);

                bucket.AddOperation(operation);
                Replace(bucket);
                return bucket;
            }

            return null;
        }


        public (BucketRecord, List<OperationRecord>) RemoveNotSatisfiedOperations(BucketRecord bucket, Agent agent)
        {
            List<OperationRecord> notSatisfiedOperations = new List<OperationRecord>();
            var bucketOperations = bucket.Operations.ToList();
            foreach (var operation in bucketOperations)
            {
                agent.DebugMessage(msg:$"In RemoveNotSatisfiedOperations check the operation {operation.Key} {operation.Operation.Name} in {bucket.Name} has" +
                                       $"| Satisfied: {operation.StartCondition.Satisfied} " +
                                       $"| ArticleProvided: {operation.StartCondition.ArticlesProvided} " +
                                       $"| PreCondition: {operation.StartCondition.PreCondition} ", CustomLogger.JOB, LogLevel.Warn);

                if (!operation.StartCondition.Satisfied)
                {
                    agent.DebugMessage(msg: $"Operation {operation.Key} in {operation.Operation.Name} in {bucket.Name} will be removed because it is not satisfied!", CustomLogger.JOB, LogLevel.Warn);

                    notSatisfiedOperations.Add(operation);
                    bucket = bucket.RemoveOperation(operation);
                }
            }
            return (bucket, notSatisfiedOperations);
        }

        public void AddOrUpdateBucketSize(M_ResourceCapability capability, TimeSpan duration)
        {
            if (CapabilityBucketSizeDictionary.TryGetValue(capability.Id, out var value)) // update
            {
                value.Duration += duration;
                CalculateBucketSize();
                return;
            }
            // Create    
            var bucketSize = new BucketSize { Duration = duration, Size = duration, Capability = capability };
            CapabilityBucketSizeDictionary.Add(capability.Id, bucketSize);
            CalculateBucketSize();
        }

        public void DecreaseBucketSize(int capabilityId, TimeSpan duration)
        {
            if (CapabilityBucketSizeDictionary.TryGetValue(capabilityId, out var value)) // update
            {
                value.Duration -= duration;
                if (value.Duration < TimeSpan.Zero)
                    throw new Exception("Capability capacity for " + value.Capability.Id + " negativ");
                CalculateBucketSize();
            }

        }
        
        private void CalculateBucketSize()
        {
            var sumCapability = CapabilityBucketSizeDictionary.Sum(x => x.Value.Duration).TotalSeconds;
            if (sumCapability == 0) return; // no work left to calculate bucket size.
            foreach (var bucketSizeTuple in CapabilityBucketSizeDictionary)
            {
                double ts = bucketSizeTuple.Value.Duration.TotalSeconds;
                bucketSizeTuple.Value.Ratio = ts / sumCapability;
                bucketSizeTuple.Value.Size = TimeSpan.FromSeconds(Math.Round(bucketSizeTuple.Value.Ratio * MaxBucketSize.TotalSeconds, 0));
                if (bucketSizeTuple.Value.Size < TimeSpan.FromMinutes(60)) //TODO Probably not necessary
                {
                    bucketSizeTuple.Value.Size = TimeSpan.FromMinutes(60);
                }
            }
        }

        public TimeSpan GetBucketSize(int capabilityId)
        {
            if (CapabilityBucketSizeDictionary.TryGetValue(capabilityId, out var value))
            {
                if (value.Capability.IsBatchAble)
                {
                    return value.Duration;
                }
                return value.Size;
            }
            // else
            throw new Exception("No bucket size Found!");
        }
    }
}
