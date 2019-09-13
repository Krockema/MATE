using System;
using System.Collections.Generic;
using System.Linq;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using static FBuckets;
using static FOperations;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    /// <summary>
    /// hold all operations in buckets, ignoring if operation preconditions are satisfied or not
    /// </summary>
    public class BucketManager
    {
        private List<FBucket> _buckets { get; set; } = new List<FBucket>();

        internal FBucket CreateBucket(FOperation fOperation, long currentTime)
        {
            var bucket = MessageFactory.ToBucketItem(fOperation, currentTime);
            _buckets.Add(bucket);
            return bucket;
        }

        internal FBucket GetBucketById(Guid key)
        {
            return _buckets.Single(x => x.Key == key);
        }

        internal void Replace(FBucket bucket)
        {
            _buckets.Replace(bucket);
        }

        internal FOperation GetOperationByKey(Guid operationKey)
        {
            FOperation operation = null;
            foreach (var bucket in _buckets)
            {
                var op = bucket.Operations.Single(x => x.Key == operationKey);
                if (op != null)
                {
                    operation = op;
                }
            }
            return operation;
        }

        internal FBucket GetBucketByOperationKey(Guid operationKey)
        {
            return _buckets.Single(x => x.Operations.Any(y => y.Key == operationKey));
        }

        internal void Remove(Guid operationKey)
        {
            var operation = GetOperationByKey(operationKey);
            var bucket = GetBucketByOperationKey(operationKey);

            bucket.RemoveOperation(operation);

        }

        internal FBucket AddToBucket(Guid bucketKey, FOperation fOperation)
        {
            var bucket = _buckets.Single(x => x.Key == bucketKey);

            if (bucket == null) throw new Exception($"Bucket {bucketKey} does not exits");

            bucket.AddOperation(fOperation);
            _buckets.Replace(bucket);

            return bucket;
        }

        internal FBucket FindBucket(FOperation fOperation, long currentTime)
        {
            var bucket = _buckets.SingleOrDefault(x => x.Tool.Id == fOperation.Tool.Id 
                                                       && !x.IsFixPlanned 
                                                       && x.BackwardEnd > fOperation.BackwardEnd 
                                                       && x.ForwardStart < fOperation.ForwardStart);

            return bucket;

        }
    }
}
