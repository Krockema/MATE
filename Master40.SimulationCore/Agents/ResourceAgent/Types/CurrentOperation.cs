using static FOperations;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class CurrentOperation
    {

        public FOperation Operation { get; set; } = null;

        public long StartAt { get; set; } = 0;

        public void Set(FOperation operation, long currentTime)
        {
            Operation = operation;
            StartAt = currentTime;
        }

        public void Reset()
        {
            Operation = null;
            StartAt = 0;
        }
    }
}
