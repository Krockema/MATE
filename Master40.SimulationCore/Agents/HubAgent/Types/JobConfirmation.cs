using Akka.Actor;
using Master40.DB.DataModel;
using static FBuckets;
using static FJobConfirmations;
using static FScopeConfirmations;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class JobConfirmation
    {
        public IJob Job { get; set; }
        public M_ResourceCapabilityProvider CapabilityProvider { get; set; }
        public FScopeConfirmation ScopeConfirmation { get; set; }
        public bool IsConfirmed => CapabilityProvider != null;
        public bool IsRequestedToDissolve { get; set; }
        public IActorRef JobAgentRef { get; private set; }
        public JobConfirmation(IJob job)
        {
            Job = job;
            CapabilityProvider = null;
            ScopeConfirmation = null;
            IsRequestedToDissolve = false;
        }

        public bool IsFixPlanned => ((FBucket) Job).IsFixPlanned;

        public FJobConfirmation ToImmutable()
        {
            return new FJobConfirmation(Job, ScopeConfirmation, Job.Key, CapabilityProvider, JobAgentRef);
        }
        
        public void SetJobAgent(IActorRef agentRef)
        {
            JobAgentRef = agentRef;
        }

        public void ResetConfirmation()
        {
            CapabilityProvider = null;
            ScopeConfirmation = null;
        }
    }
}