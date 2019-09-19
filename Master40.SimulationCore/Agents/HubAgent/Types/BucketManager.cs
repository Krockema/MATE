using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using static FBuckets;
using static FOperations;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    /// <summary>
    /// hold all operations in buckets, ignoring if operation preconditions are satisfied or not
    /// </summary>
    public class BucketManager
    {
        private List<FBucket> _buckets { get; set; } = new List<FBucket>();

        internal FBucket CreateBucket(FOperation fOperation, IActorRef hubAgent, long currentTime)
        {
            var bucket = MessageFactory.ToBucketItem(fOperation, hubAgent, currentTime);
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

        /// <summary>
        /// Find the operation in all buckets 
        /// </summary>
        /// <param name="operationKey"></param>
        /// <returns></returns>
        internal FOperation GetOperationByKey(Guid operationKey)
        {
            FOperation operation = null;
            foreach (var bucket in _buckets)
            {
                var op = bucket.Operations.FirstOrDefault(x => x.Key == operationKey);
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

            bucket = bucket.RemoveOperation(operation);
            _buckets.Replace(bucket);
        }

        internal FBucket AddToBucket(Guid bucketKey, FOperation fOperation)
        {
            var bucket = _buckets.Single(x => x.Key == bucketKey);

            if (bucket == null) throw new Exception($"Bucket {bucketKey} does not exits");

            bucket = bucket.AddOperation(fOperation);
            _buckets.Replace(bucket);

            return bucket;
        }

        internal FBucket FindOrCreateBucket(FOperation fOperation, IActorRef hubAgent, long currentTime)
        {
            var matchingBuckets = FindAllMatching(fOperation);

            var bucket = FindBestMatching(matchingBuckets, fOperation);

            if (bucket == null)
            {
                bucket = CreateBucket(fOperation, hubAgent, currentTime);
                _buckets.Add(bucket);
            }

            return bucket;

        }

        internal List<FBucket> FindAllMatching(FOperation fOperation)
        {
            return _buckets.Where(x => x.Tool.Id == fOperation.Tool.Id && !x.IsFixPlanned).ToList();
        }

        internal FBucket FindBestMatching(List<FBucket> buckets, FOperation operation)
        {
            
            // Simple Rule
            var bucket = buckets.FirstOrDefault(x => ((IJob)x).Duration + operation.Operation.Duration <= 30);

            // TODO Complex Rule
            // Gest First ELement of CurrentBucket
            // min(bucket.forward.start,Operation.Forward.Start) + bucket.Duration + Operation.Duration <= first.BackWardStart
            // else try Next Bucket.

            return bucket;
        }
    }
}
