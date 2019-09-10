using System;
using System.Collections.Generic;
using System.Text;
using Akka.Pattern;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class JobInProgress
    {
        public IJob Current { get; private set; }

        public long ResourceIsBusyUntil { get; set; } = 0;
        public bool IsSet => Current != null;

        public bool Set(IJob job, long currentTime)
        {
            if (IsSet)
                return false;
            Current = job;
            ResourceIsBusyUntil = currentTime + job.Duration;
            return true;
        }

        public void Reset()
        {
            Current = null;
            ResourceIsBusyUntil = 0;
        }
    }
}
