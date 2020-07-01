using System;
using System.Collections.Generic;
using System.Text;
using AkkaSim.Definitions;
using Master40.SimulationCore.Agents.HubAgent;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class Scope
    {
        public Guid BucketKey { get; }
        public long Start { get; }
        public long End { get; }
        public long Duration { get; private set; }
        public long Priority { get; } 
        public bool Ready { get; private set; }
        public bool Fix { get; private set; } = false;

        public long GetScope() => End - Start;
        public void SetFix() => Fix = true;
        public void SetReady() => Ready = true;

        public Scope(Guid bucketKey, long bucketStart, long bucketEnd, long duration, long proPriority)
        {
            BucketKey = bucketKey;
            Start = bucketStart;
            End = bucketEnd;
            Duration = duration;
        }

        public void SetDuration(long duration)
        {
            Duration = duration;
        }

    }
}
