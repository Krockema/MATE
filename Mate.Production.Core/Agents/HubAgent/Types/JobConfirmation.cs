﻿using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Environment.Records.Scopes;

namespace Mate.Production.Core.Agents.HubAgent.Types
{
    public class JobConfirmation
    {
        public IJob Job { get; set; }
        public M_ResourceCapabilityProvider CapabilityProvider { get; set; }
        public ScopeConfirmationRecord ScopeConfirmation { get; set; }
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

        public bool IsFixPlanned => ((BucketRecord) Job).IsFixPlanned;

        public IConfirmation ToImmutable()
        {
            return new JobConfirmationRecord(Job, ScopeConfirmation, Job.Key, CapabilityProvider, JobAgentRef);
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