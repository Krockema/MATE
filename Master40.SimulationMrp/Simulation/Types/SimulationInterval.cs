namespace Master40.SimulationMrp.Simulation.Types
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
