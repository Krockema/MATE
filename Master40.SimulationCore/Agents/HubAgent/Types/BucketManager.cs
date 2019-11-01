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

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    /// <summary>
    /// hold all operations in buckets, ignoring if operation preconditions are satisfied or not
    /// </summary>
    public class BucketManager
    {
        private List<FBucket> _buckets { get; set; } = new List<FBucket>();

        private long _maxBucketSize { get; set; }

        public BucketManager(long maxBucketSize)
        {
            _maxBucketSize = maxBucketSize;
        }

        public FBucket CreateBucket(FOperation fOperation, IActorRef hubAgent, long currentTime)
        {
            var bucket = MessageFactory.ToBucketScopeItem(fOperation, hubAgent, currentTime);
            _buckets.Add(bucket);
            return bucket;
        }

        public FBucket GetBucketById(Guid key)
        {
            return _buckets.SingleOrDefault(x => x.Key == key);
        }

        public void Replace(FBucket bucket)
        {
            _buckets.Replace(bucket);
        }

        /// <summary>
        /// Find the operation in all buckets 
        /// </summary>
        /// <param name="operationKey"></param>
        /// <returns></returns>
        public FOperation GetOperationByKey(Guid operationKey)
        {
            FOperation operation = null;
            foreach (var bucket in _buckets)
            {
                var op = bucket.Operations.SingleOrDefault(x => x.Key == operationKey);
                if (op != null)
                {
                    operation = op;
                }
            }
            return operation;
        }

        public FBucket GetBucketByOperationKey(Guid operationKey)
        {
            return _buckets.SingleOrDefault(x => x.Operations.Any(y => y.Key == operationKey));
        }

        public bool Remove(FBucket bucket)
        {
            return _buckets.Remove(bucket);
        }

        public void RemoveOperation(Guid operationKey)
        {
            var operation = GetOperationByKey(operationKey);
            var bucket = GetBucketByOperationKey(operationKey);

            bucket = bucket.RemoveOperation(operation);
            _buckets.Replace(bucket);
            //TODO delete this one after working
            //System.Diagnostics.Debug.WriteLine($"{bucket.Name} has removed operation {operation.Operation.Name} {operation.Key} und has now {bucket.Operations.Count} operations left");
        }

        public FBucket Add(FBucket bucket, FOperation fOperation)
        {
            if (bucket == null) throw new Exception($"Bucket {bucket.Name} does not exits");

            bucket = bucket.AddOperation(fOperation);
            _buckets.Replace(bucket);

            return bucket;
        }

        public List<FOperation> ModifyBucket(FOperation fOperation)
        {
            List<FOperation> operationsToModify = new List<FOperation>();

            var bucketsToModify = FindAllBucketsLaterForwardStart(fOperation);

            if (bucketsToModify != null)
            {
                //Remove Start to requeue the buckets order by FS?
                foreach (var bucket in bucketsToModify)
                {
                    operationsToModify.AddRange(bucket.Operations);
                    _buckets.Remove(bucket);
                }

            }

            return operationsToModify;
        }



        /// <summary>
        /// Add the operation to an existing matching bucket or otherwise creates a new one
        /// </summary>
        /// <param name="fOperation"></param>
        /// <param name="hubAgent"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public FBucket AddToBucket(FOperation fOperation, IActorRef hubAgent, long currentTime)
        {
            var matchingBuckets = FindAllWithSameTool(fOperation);

            FBucket bucket = null;

            if (matchingBuckets != null)
            {
                //Add
                bucket = FindEarliestAndLatestScopeMatching(matchingBuckets, fOperation);
                if (bucket != null)
                {
                    bucket = Add(bucket: bucket, fOperation: fOperation);
                    return bucket;
                }

            }
            //need to create a new one
            return bucket;

        }

        public List<FBucket> FindAllWithSameTool(FOperation fOperation)
        {
            return _buckets.Where(x => x.Tool.Name == fOperation.Tool.Name && !x.IsFixPlanned).ToList();
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

        public List<FBucket> FindAllBucketsLaterForwardStart(FOperation operation)
        {
            var matchingBuckets = FindAllWithSameTool(operation);
            return matchingBuckets.Where(x => x.ForwardStart > operation.ForwardStart).ToList();
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
                if (HasCapacityLeft(bucket, operation) && HasLaterForwardStart(bucket, operation) && ((IJob)bucket).Duration + operation.Operation.Duration < _maxBucketSize)
                {
                    matchingBuckets.Add(bucket);
                }

            }

            var matchingBucket = GetBucketWithMostLeftCapacity(matchingBuckets);

            return matchingBucket;
        }


        public FBucket GetBucketWithMostLeftCapacity(List<FBucket> buckets)
        {
            buckets = buckets.Where(x => x.IsFixPlanned.Equals(false)).ToList().OrderBy(x => x.GetCapacityLeft).ToList();

            /*foreach (var bucket in buckets)
            {
                System.Diagnostics.Debug.WriteLine($"{bucket.Name} has {bucket.GetCapacityLeft} left");
            }*/

            return buckets.FirstOrDefault();
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
            var _bucket = bucket;

            foreach (var operation in operationsToRemove)
            {
                _bucket = _bucket.RemoveOperation(operation);
            }

            _buckets.Replace(_bucket);

            return _bucket;
        }

        public FBucket SetOperationStartCondition(Guid operationKey, FUpdateStartCondition startCondition)
        {
            var bucket = GetBucketByOperationKey(operationKey);

            var operation = bucket.Operations.Single(x => x.Key == operationKey);
            operation.SetStartConditions(startCondition);

            _buckets.Replace(bucket);
            return bucket;
        }

        public bool SetBucketSatisfied(FBucket bucket)
        {
            bool preConditons = bucket.Operations.All(x => x.StartConditions.PreCondition);
            bool articlesProvided = bucket.Operations.All(x => x.StartConditions.ArticlesProvided);

            bucket.SetStartConditions(startCondition: new FUpdateStartCondition(operationKey: bucket.Key, preCondition: preConditons, articlesProvided: articlesProvided));
            _buckets.Replace(bucket);
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
    }
}
