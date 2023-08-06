using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records.Interfaces;
using System;

namespace Mate.Production.Core.Environment.Records.Scopes
{
    public record JobConfirmationRecord(
        IJob Job,
        ScopeConfirmationRecord ScopeConfirmation,
        Guid Key,
        M_ResourceCapabilityProvider CapabilityProvider,
        IActorRef JobAgentRef) : IConfirmation
    {
        public bool IsReset => ScopeConfirmation.Equals(null);

        public TimeSpan Duration => throw new NotImplementedException();

        public IConfirmation UpdateJob(IJob job) => this with { Job = job };

        public IConfirmation UpdateScopeConfirmation(ScopeConfirmationRecord scopeConfirmations) =>
            this with { ScopeConfirmation = scopeConfirmations };
    }
}


