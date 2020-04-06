using System.Collections.Generic;
using Akka.Actor;
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
        public bool IsConfirmed => SetupDefinition.SetupKey != -1;

        public JobConfirmation(IJob job)
        {
            Job = job;
            SetupDefinition = new FSetupDefinition(-1, new List<IActorRef>());
            Schedule = -1;
        }

        public string RequiresCapability => Job.RequiredCapability.Name;

        public bool IsFixPlanned => ((FBucket) Job).IsFixPlanned;

        public FJobConfirmation ToImmutable()
        {
            return new FJobConfirmation(Job, Schedule, Job.Duration ,SetupDefinition);
        }

        public void ResetConfirmation()
        {
            SetupDefinition = new FSetupDefinition(-1, new List<IActorRef>()); 
            Schedule = -1;
        }
    }
}