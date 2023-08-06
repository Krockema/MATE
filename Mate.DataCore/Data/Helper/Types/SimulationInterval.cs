using Mate.DataCore.Data.WrappersForPrimitives;
using System;

namespace Mate.DataCore.Data.Helper.Types
{
    public class SimulationInterval
    {
        public SimulationInterval(DateTime startAt, TimeSpan interval)
        {
            StartAt = startAt;
            Interval = interval;
        }
        public DateTime StartAt { get; }
        public TimeSpan Interval { get;  }
        public DateTime EndAt => StartAt + Interval;
        
        public bool IsWithinInterval(DateTime dueTime)
        {
            bool isInInterval = dueTime <= EndAt &&
                                dueTime >= StartAt;
            return isInInterval;
        }

        public bool IsBeforeInterval(DateTime dueTime)
        {
            return dueTime < StartAt;
        }

        public DateTime GetStart()
        {
            return StartAt;
        }
        
        public DateTime GetEnd()
        {
            return EndAt;
        }
    }
    
    
}
