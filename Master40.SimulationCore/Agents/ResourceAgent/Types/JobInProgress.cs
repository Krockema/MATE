using System;
using System.Collections.Generic;
using System.Text;
using static FBuckets;
using static FOperations;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobInProgress
    {
        public IJob Current { get; private set; }
        public long StartTime { get; private set; }
        public long ResourceIsBusyUntil { get; set; } = 0;
        public bool IsSet => Current != null;
        public void SetStartTime(long time) => StartTime = time;

        public bool Set(IJob job, long currentTime)
        {
            if (IsSet)
                return false;
            Current = job;
            ResourceIsBusyUntil = currentTime + job.Duration;
            StartTime = currentTime;
            return true;
        }

        public void Reset()
        {
            Current = null;
            ResourceIsBusyUntil = 0;
        }

        internal void RemoveOperation(FOperation operation)
        {
            var bucket = (FBucket) Current;
            Current = bucket.RemoveOperation(operation);

        }
    }
}
