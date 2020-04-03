using static FBuckets;
using static FJobConfirmations;
using static FSetupDefinitions;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class JobConfirmation
    {
        public IJob Job { get; set; }
        public FSetupDefinition SetupDefinition { get; set; }
        public long Schedule { get; set; }

        public JobConfirmation(IJob job)
        {
            Job = job;
            SetupDefinition = null;
            Schedule = -1;
        }

        public string RequiresCapability => Job.RequiredCapability.Name;

        public bool IsFixPlanned => ((FBucket) Job).IsFixPlanned;

        public FJobConfirmation ToImutable()
        {
            return new FJobConfirmation(Job, Schedule, SetupDefinition);
        }

        public void ResetConfirmation()
        {
            SetupDefinition = null;
            Schedule = -1;
        }
    }
}