using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class BucketScopeQueue
    {
        private Queue<BucketScope> _bucketScopeQueue = new Queue<BucketScope>();

        private long _limit = 0L;

        private long _size = 0L;

        public BucketScopeQueue(long limit)
        {
            _limit = limit;
        }

        public void Dequeue(BucketScope bucketScope)
        {

        }


        public void Enqueue(BucketScope bucketScope)
        {
            _size += bucketScope._duration;
            _bucketScopeQueue.Enqueue(bucketScope);
        }

        public bool HasCapacityLeft(BucketScope bucketScope)
        {
            return _limit < _size + bucketScope._duration;
        }

        internal long GetQueueableScope(FBucketScopes.FBucketScope bucketScope)
        {
            var queueableTime = -1L;
            return queueableTime;
        }
    }
}
