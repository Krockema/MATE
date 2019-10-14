using System;
using System.Collections.Generic;
using System.Text;
using static FBucketScopes;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    /// <summary>
    /// Queue for Planing the Scopes
    /// </summary>
    public class BucketScopeQueue
    {
        public Queue<BucketScope> bucketScopes { get; private set; } = new Queue<BucketScope>();

        private long limit = 0L;

        private long size = 0L;

        public BucketScopeQueue(long limit)
        {
            this.limit = limit;
        }

        public void Dequeue(BucketScope bucketScope)
        {

        }


        public void Enqueue(BucketScope bucketScope)
        {
            size += bucketScope._duration;
            bucketScopes.Enqueue(bucketScope);
        }

        public bool HasCapacityLeft(BucketScope bucketScope)
        {
            return limit < size + bucketScope._duration;
        }

        internal void GetQueueableScope()
        {

        }

        internal bool HasQueueAbleJobs()
        {

            //Check if any Scope is ready and can
                
            //Sort by priority put 
            throw new NotImplementedException();
        }

       
    }
}
