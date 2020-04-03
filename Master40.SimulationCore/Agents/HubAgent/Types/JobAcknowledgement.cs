using static FJobConfirmations;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class JobAcknowledgement
    {
        public JobAcknowledgement(FJobConfirmation jobConfirmation)
        {
            JobConfirmation = jobConfirmation;
        }

        public FJobConfirmation JobConfirmation { get;  }
        public bool ToReplace => JobConfirmation != null;
    }
}
