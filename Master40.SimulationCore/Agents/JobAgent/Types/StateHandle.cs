using Master40.DB.Nominal;

namespace Master40.SimulationCore.Agents.JobAgent.Types
{
    public class StateHandle
    {
        public JobState CurrentState { get; set; }
        public bool Setup { get; }
        public bool Processing { get; }

        public StateHandle(JobState currentState, bool setup, bool processing)
        {
            CurrentState = currentState;
            Setup = setup;
            Processing = processing;
        }


    }
}
