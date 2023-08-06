using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records.Interfaces;
using System;

namespace Mate.Production.Core.Environment.Records.Queueing
{
    public record QeueingSetupRecord
    (
        Guid Key,
        int ResourceId,
        IJob Job,
        Guid JobKey,
        string JobName,
        TimeSpan Duration,
        IActorRef Hub,
        M_ResourceCapabilityProvider CapabilityProvider,
        string JobType
    ) : IQueueingJob; 
}
