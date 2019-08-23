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

        public bool IsSet => Current != null;

        public bool Set(IJob job)
        {
            if (IsSet)
                return false;
            Current = job;
            return true;
        }

        public void Reset()
        {
            Current = null;
        }
    }
}
