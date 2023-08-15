using Akka.Actor;
using Mate.Production.Core.Environment.Records.Interfaces;
using System;

namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record OperationResultRecord( 
        Guid Key,
        DateTime CreationTime,
        DateTime Start,
        DateTime End,
        TimeSpan OriginalDuration,
        IActorRef ProductionAgent,
        string CapabilityProvider
    ) : IKey, IJobResult
    {
        public IJobResult FinishedAt(DateTime timestamp) =>
            this with { End = timestamp,
                         OriginalDuration = this.End - this.Start };
        public OperationResultRecord UpdatePoductionAgent(IActorRef p) => this with { ProductionAgent = p };
}
}
