using System;
using System.Collections.Generic;
using System.Text;
using Master40.SimulationCore.Agents.HubAgent;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class BucketScope
    {
        public Guid _bucketKey { get; }
        public long _bucketStart { get; }
        public long _bucketEnd { get; }
        public long _duration { get; private set; }

        public bool _fix { get; private set; } = false;

        public void SetFix() => _fix = true;

        public BucketScope(Guid bucketKey, long bucketStart, long bucketEnd, long duration)
        {
            _bucketKey = bucketKey;
            _bucketStart = bucketStart;
            _bucketEnd = bucketEnd;
            _duration = duration;
        }

        public void SetDuration(long duration)
        {
            _duration = duration;
        }


    }
}
