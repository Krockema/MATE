using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using static FBuckets;
using static FOperations;
using static IJobs;
using static FUpdateStartConditions;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.JobAgent;
using static FJobConfirmations;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    /// <summary>
    /// hold all operations in buckets, ignoring if operation preconditions are satisfied or not
    /// </summary>
    public class BucketManager
    {
        private List<JobConfirmation>  _jobConfirmations { get; set; } = new List<JobConfirmation>();
        public Dictionary<int, BucketSize> CapabilityBucketSizeDictionary { get; set; } = new Dictionary<int, BucketSize>();

        private long MaxBucketSize { get; set; }
        
        public BucketManager(long maxBucketSize)
        {
            MaxBucketSize = maxBucketSize;
        }

        public JobConfirmation CreateBucket(FOperation fOperation, Agent agent)
        {

            var bucketSize = GetBucketSize(fOperation.RequiredCapability.Id);
            var bucket = MessageFactory.ToBucketScopeItem(fOperation, agent.Context.Self, agent.CurrentTime, bucketSize);
            var jobConfirmation = new JobConfirmation(bucket);
            jobConfirmation.SetJobAgent(agent.Context.ActorOf(Job.Props(agent.ActorPaths, jobConfirmation.ToImmutable(), agent.CurrentTime, agent.DebugThis, agent.Context.Self)
                                       , $"JobAgent({bucket.Key})"));
            _jobConfirmations.Add(jobConfirmation);
            return jobConfirmation;
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

        public bool Remove(Guid bucketKey, Agent agent)
        {
            var jobConfirmation = GetConfirmationByBucketKey(bucketKey);
            var removed = _jobConfirmations.Remove(jobConfirmation);
            if (removed)
                agent.Send(Job.Instruction.TerminateJob.Create(jobConfirmation.JobAgentRef));
            return removed;
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
                   GetBucketSize(bucket.RequiredCapability.Id);

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

        public void AddOrUpdateBucketSize(M_ResourceCapability capability, long duration)
        {
            if (CapabilityBucketSizeDictionary.TryGetValue(capability.Id, out var value)) // update
            {
                value.Duration += duration;
                CalculateBucketSize();
                return;
            }
            // Create    
            var bucketSize = new BucketSize {Duration = duration, Size = duration, Capability = capability};
            CapabilityBucketSizeDictionary.Add(capability.Id, bucketSize);
            CalculateBucketSize();
        }

        public void DecreaseBucketSize(int capabilityId, long duration)
        {
            if (CapabilityBucketSizeDictionary.TryGetValue(capabilityId, out var value)) // update
            {
                value.Duration -= duration;
                if (value.Duration < 0)
                    throw new Exception("Capability capacity for " + value.Capability.Id + " negativ");
                CalculateBucketSize();
            }

        }
        
        private void CalculateBucketSize()
        {
            var sumCapability = CapabilityBucketSizeDictionary.Sum(x => x.Value.Duration);
            if (sumCapability == 0) return; // no work left to calculate bucket size.
            foreach (var bucketSizeTuple in CapabilityBucketSizeDictionary)
            {

                bucketSizeTuple.Value.Ratio = (double)bucketSizeTuple.Value.Duration / sumCapability;
                bucketSizeTuple.Value.Size = Convert.ToInt64(Math.Round(bucketSizeTuple.Value.Ratio * MaxBucketSize, 0));
                if (bucketSizeTuple.Value.Size < 60) // to ensure a minimal bucket size. // TODO May Obsolet
                {
                    bucketSizeTuple.Value.Size = 60;
                }
            }
        }

        public long GetBucketSize(int capabilityId)
        {
            if (CapabilityBucketSizeDictionary.TryGetValue(capabilityId, out var value))
                return value.Size;
            // else
            throw new Exception("No bucket size Found!");
        }
    }
}
