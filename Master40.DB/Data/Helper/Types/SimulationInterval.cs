using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.DB.Data.Helper.Types
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
        
        public bool IsWithinInterval(DueTime dueTime)
        {
            bool isInInterval = dueTime.GetValue() <= EndAt &&
                                dueTime.GetValue() >= StartAt;
            return isInInterval;
        }

        public bool IsBeforeInterval(DueTime dueTime)
        {
            return dueTime.GetValue() < StartAt;
        }

        public DueTime GetStart()
        {
            return new DueTime((int)StartAt);
        }
        
        public DueTime GetEnd()
        {
            return new DueTime((int)EndAt);
        }
    }
    
    
}
