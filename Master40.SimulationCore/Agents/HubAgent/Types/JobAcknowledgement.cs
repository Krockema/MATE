using System;
using static FBuckets;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class JobAcknowledgement
    {
        public JobAcknowledgement(Guid bucketKey, FBucket bucket)
        {
            Bucket = bucket;
            JobKey = bucketKey;
        }

        public FBucket Bucket { get;  }
        public Guid JobKey { get;  }
        public bool ToReplace => Bucket != null;
    }
}
