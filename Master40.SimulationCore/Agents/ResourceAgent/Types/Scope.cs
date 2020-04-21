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
        public double Priority { get; } 
        public bool Ready { get; private set; }
        public bool Fix { get; private set; } = false;

        public long GetScope() => End - Start;
        public void SetFix() => Fix = true;
        public void SetReady() => Ready = true;

        public Scope(Guid bucketKey, long bucketStart, long bucketEnd, long duration, long priority)
        {
            BucketKey = bucketKey;
            Start = bucketStart;
            End = bucketEnd;
            Duration = duration;
            Priority = priority;
        }

        public Scope(long start, long end, double priority)
        {
            Start = start;
            End = end;
            Priority = priority;
        }

        public void SetDuration(long duration)
        {
            Duration = duration;
        }

    }
}
