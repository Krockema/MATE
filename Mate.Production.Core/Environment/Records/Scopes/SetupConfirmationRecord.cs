using Mate.DataCore.DataModel;
using System;
using Akka.Actor;
using Mate.Production.Core.Environment.Records.Interfaces;

namespace Mate.Production.Core.Environment.Records.Scopes
{
    public record SetupConfirmationRecord
    (
        IJob Job,
        ScopeConfirmationRecord ScopeConfirmation,
        Guid Key,
        TimeSpan Duration,
        M_ResourceCapabilityProvider CapabilityProvider,
        IActorRef JobAgentRef) : IConfirmation {
            public bool IsReset => this.ScopeConfirmation.Equals(null);
            public IConfirmation UpdateJob(IJob job) => this with { Job = job };
    }
}
