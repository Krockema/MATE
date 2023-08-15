using Akka.Actor;
using Mate.Production.Core.Environment.Records.Interfaces;
using System;

namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record ProductionResultRecord(Guid Key
            , Guid TrackingId
            , DateTime CreationTime
            , DateTime CustomerDue
            , IActorRef ProductionRef
            , decimal Amount) : IKey;
}