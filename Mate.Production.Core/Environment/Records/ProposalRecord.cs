using Akka.Actor;
using Mate.Production.Core.Environment.Records.Scopes;
using System;

namespace Mate.Production.Core.Environment.Records
{
    public record ProposalRecord
    (
            object PossibleSchedule,
            PostponedRecord Postponed,
            int CapabilityId,
            IActorRef ResourceAgent,
            Guid JobKey
    );
}
