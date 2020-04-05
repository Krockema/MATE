using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using static FBuckets;
using static FOperations;
using static IJobs;
using static FUpdateStartConditions;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    /// <summary>
    /// hold all operations in buckets, ignoring if operation preconditions are satisfied or not
    /// </summary>
    public class BucketManager
    {
        private List<JobConfirmation>  _jobConfirmations { get; set; } = new List<JobConfirmation>();
        public Dictionary<M_ResourceCapability, long> CapabilityBucketSizeDictionary { get; set; } = new Dictionary<M_ResourceCapability, long>();

        private long MaxBucketSize { get; set; }
        
        public BucketManager(long maxBucketSize)
        {
            MaxBucketSize = maxBucketSize;
        }

        public FBucket CreateBucket(FOperation fOperation, IActorRef hubAgent, long currentTime)
        {
            var bucket = MessageFactory.ToBucketScopeItem(fOperation, hubAgent, currentTime);
            _jobConfirmations.Add(new JobConfirmation(bucket));
            return bucket;
        }

        public FBucket GetBucketByBucketKey(Guid bucketKey)
        {
            return _jobConfirmations.SingleOrDefault(x => x.Job.Key == bucketKey)?.Job as FBucket;
        }
        public JobConfirmation GetConfirmationByBucketKey(Guid bucketKey)
        {
            return _jobConfirmations.SingleOrDefault(x => x.Job.Key == bucketKey);
        }

        public void Replace(FBucket bucket)
        {
            _jobConfirmations.SingleOrDefault(x => x.Job.Key == bucket.Key).Job = bucket;
        }

        /// <summary>
        /// Find the operation in all buckets 
        /// </summary>
        /// <param name="operationKey"></param>
        /// <returns></returns>
        public FOperation GetOperationByOperationKey(Guid operationKey)
        {
            var bucket = GetBucketByOperationKey(operationKey);
            return bucket?.Operations.Single(x => x.Key == operationKey);
        }

        public FBucket GetBucketByOperationKey(Guid operationKey)
        {
            return _jobConfirmations.SingleOrDefault(x => ((FBucket)x.Job).Operations.Any(y => y.Key == operationKey))?.Job as FBucket;
        }

        public bool Remove(Guid bucketKey)
        {
            return 1 == _jobConfirmations.RemoveAll(x => x.Job.Key == bucketKey);
        }

        /// <summary>
        /// Returns the removed operation
        /// </summary>
        /// <param name="operationKey"></param>
        /// <returns></returns>
        public FOperation RemoveOperation(Guid operationKey)
        {
            var bucket = GetBucketByOperationKey(operationKey);
            var operation = GetOperationByOperationKey(operationKey);
            bucket = bucket.RemoveOperation(operation);
            Replace(bucket);
            return operation;
        }

        public FBucket Add(FBucket bucket, FOperation fOperation)
        {
            if (bucket == null) throw new Exception($"Bucket {bucket.Name} does not exits");

            bucket = bucket.AddOperation(fOperation);
            Replace(bucket);

            return bucket;
        }

        /// <summary>
        /// Add the operation to an existing matching bucket or otherwise creates a new one
        /// </summary>
        /// <param name="fOperation"></param>
        /// <param name="hubAgent"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public FBucket AddToBucket(FOperation fOperation)
        {
            var matchingBuckets = FindAllWithEqualCapability(fOperation).Select(x => x.Job).Cast<FBucket>().ToList();

            FBucket bucket = null;

            if (matchingBuckets != null)
            {
                //Add
                bucket = FindEarliestAndLatestScopeMatching(matchingBuckets, fOperation);
                if (bucket != null)
                {
                    bucket = Add(bucket: bucket, fOperation: fOperation);
                    Replace(bucket);
                    return bucket;
                }

            }
            //need to create a new one
            return bucket;

        }

        public List<JobConfirmation> FindAllWithEqualCapability(FOperation fOperation)
        {
            return _jobConfirmations.Where(x => x.Job.RequiredCapability.Name == fOperation.RequiredCapability.Name 
                                                         && !x.IsFixPlanned)
                                    .ToList();
        }

        /// <summary>
        /// Return null if no matching bucket exists
        /// </summary>
        /// <param name="buckets"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public FBucket FindSimpleRuleMatching(List<FBucket> buckets, FOperation operation)
        {
            // Simple Rule
            return buckets.FirstOrDefault(x => ((IJob)x).Duration + operation.Operation.Duration + operation.Operation.AverageTransitionDuration <= 30);

        }

        public List<JobConfirmation> FindAllBucketsLaterForwardStart(FOperation operation)
        {
            var matchingBuckets = FindAllWithEqualCapability(operation);
            return matchingBuckets.Where(x => x.Job.ForwardStart > operation.ForwardStart).ToList();
        }

        /// <summary>
        /// Return null if no matching bucket exits
        /// </summary>
        /// <param name="buckets">gets a List of all bi</param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public FBucket FindEarliestAndLatestScopeMatching(List<FBucket> buckets, FOperation operation)
        {
            List<FBucket> matchingBuckets = new List<FBucket>();

            foreach (var bucket in buckets)
            {
                var maxbucket = ExceedMaxBucketSize(bucket, operation);

                if (!maxbucket)
                {
                    //System.Diagnostics.Debug.WriteLine($"Max bucket size exceed");
                }

                if (HasCapacityLeft(bucket, operation) && HasLaterForwardStart(bucket, operation) && maxbucket)
                {
                    matchingBuckets.Add(bucket);
                }

            }

            var matchingBucket = GetBucketWithMostLeftCapacity(matchingBuckets);

            return matchingBucket;
        }

        private bool ExceedMaxBucketSize(FBucket bucket,FOperation operation)
        {
            return ((IJob) bucket).Duration + operation.Operation.Duration <=
                   GetCalculatedBucketSize(bucket.RequiredCapability);

        }

        public FBucket GetBucketWithMostLeftCapacity(List<FBucket> buckets)
        {
            var bucket = buckets.Where(x => !x.IsFixPlanned).ToList().OrderBy(x => x.GetCapacityLeft).ToList().FirstOrDefault();

            return bucket;
        }


        public bool HasCapacityLeft(FBucket bucket, FOperation operation)
        {
            return bucket.BackwardStart - bucket.ForwardStart > ((IJob)bucket).Duration + operation.Operation.Duration;
        }


        public bool HasLaterForwardStart(FBucket bucket, FOperation operation)
        {
            return operation.ForwardStart >= bucket.ForwardStart;
        }

        public bool HasEarlierBackwardStart(FBucket bucket, FOperation operation)
        {
            return operation.BackwardStart <= bucket.BackwardStart;
        }


        internal FBucket RemoveOperations(FBucket bucket, List<FOperation> operationsToRemove)
        {
            foreach (var operation in operationsToRemove)
            {
                bucket = bucket.RemoveOperation(operation);
            }

            Replace(bucket);

            return bucket;
        }

        public bool SetOperationStartCondition(Guid operationKey, FUpdateStartCondition startCondition)
        {
            var bucket = GetBucketByOperationKey(operationKey);

            if (bucket != null)
            {
                var operation = bucket.Operations.Single(x => x.Key == operationKey);
                operation.SetStartConditions(startCondition);
                Replace(bucket);
                return true;
            }

            return false;
        }

        public bool SetBucketSatisfied(FBucket bucket)
        {
            bool preConditons = bucket.Operations.All(x => x.StartConditions.PreCondition);
            bool articlesProvided = bucket.Operations.All(x => x.StartConditions.ArticlesProvided);

            bucket.SetStartConditions(startCondition: new FUpdateStartCondition(operationKey: bucket.Key, preCondition: preConditons, articlesProvided: articlesProvided));
            Replace(bucket);
            return preConditons && articlesProvided;
        }


        public List<FOperation> GetAllNotSatifsiedOperation(FBucket bucket)
        {
            List<FOperation> notSatisfiedOperations = new List<FOperation>();
            foreach (var operation in bucket.Operations)
            {
                if (!operation.StartConditions.Satisfied)
                {
                    notSatisfiedOperations.Add(operation);
                }
            }
            return notSatisfiedOperations;
        }

        public void AddOrUpdateBucketSize(M_ResourceCapability capability, int duration)
        {
            var (key, value) = CapabilityBucketSizeDictionary.SingleOrDefault(x => x.Key.Id == capability.Id);
            if (key != null) // update
            {
                value += duration;
                CapabilityBucketSizeDictionary[key] = value;
                return;
            }    
            // Create    
            CapabilityBucketSizeDictionary.Add(capability, duration);
        }

        public void DecreaseBucketSize(M_ResourceCapability capability, int duration)
        {
            var capabilityCapacity = CapabilityBucketSizeDictionary.Single(x => x.Key.Id == capability.Id);
            var value = capabilityCapacity.Value;
            value -= duration;
            if (value < 0)
                throw new Exception("Capability capacity for " + capabilityCapacity.Key + " negativ");
            CapabilityBucketSizeDictionary[capabilityCapacity.Key] = value;
        }

        public long GetCalculatedBucketSize(M_ResourceCapability capability)
        {
            var capabilityTuple = CapabilityBucketSizeDictionary.Single(x => x.Key.Id == capability.Id);

            var sumCapability = CapabilityBucketSizeDictionary.Sum(x => x.Value);
            double capabilitySize = capabilityTuple.Value / sumCapability;
            
            //System.Diagnostics.Debug.WriteLine($"{toolCapability.Key._resourceTool.Name} {toolRatioOfCapability} % of {toolCapability.Key._resourceCapability.Name}");
            var maxBucketSize = Convert.ToInt64(Math.Round(capabilitySize * MaxBucketSize, 0));
            //TODO Maybe add min bucket size
            return maxBucketSize < 60 ? maxBucketSize = 60 : maxBucketSize;
        }
    }
}
