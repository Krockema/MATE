using Akka.Actor;
using Master40.DB.DataModel;
using System.Collections.Generic;
using static FBuckets;
using static FJobConfirmations;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class JobConfirmation
    {
        public IJob Job { get; set; }
        public M_ResourceCapabilityProvider CapabilityProvider { get; set; }
        public long Schedule { get; set; }
        public bool IsConfirmed => CapabilityProvider != null;

        public JobConfirmation(IJob job)
        {
            Job = job;
            CapabilityProvider = null;
            Schedule = -1;
        }

        public string RequiresCapability => Job.RequiredCapability.Name;

        public bool IsFixPlanned => ((FBucket) Job).IsFixPlanned;

        public FJobConfirmation ToImmutable()
        {
            return new FJobConfirmation(Job, Schedule, Job.Duration , CapabilityProvider);
        }

        public void ResetConfirmation()
        {
            CapabilityProvider = null; 
            Schedule = -1;
        }
    }
}