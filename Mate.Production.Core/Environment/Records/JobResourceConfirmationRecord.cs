using Akka.Actor;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Environment.Records.Scopes;
using System.Collections.Generic;

namespace Mate.Production.Core.Environment.Records
{
    public record JobResourceConfirmationRecord(
        IConfirmation JobConfirmation,
        Dictionary<IActorRef, ScopeConfirmationRecord> ScopeConfirmations);
}
