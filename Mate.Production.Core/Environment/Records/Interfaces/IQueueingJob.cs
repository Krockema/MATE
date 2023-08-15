using Akka.Actor;
using Mate.DataCore.DataModel;
using System;

namespace Mate.Production.Core.Environment.Records.Interfaces
{
    public interface IQueueingJob
    {
        Guid Key { get; }
        int ResourceId { get; }
        IJob Job { get; }
        Guid JobKey { get; }
        string JobName { get; }
        TimeSpan Duration { get; }
        IActorRef Hub { get; }
        M_ResourceCapabilityProvider CapabilityProvider { get; }
        string JobType { get; }
    }
}
