using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.Distributions;

namespace Zpp.Simulation.Types
{
    public class SimulationInterval
    {
        public SimulationInterval(long startAt, long interval)
        {
            StartAt = startAt;
            Interval = interval;
        }
        public long StartAt { get; }
        public long Interval { get;  }
        public long EndAt => StartAt + Interval;
    }
}
