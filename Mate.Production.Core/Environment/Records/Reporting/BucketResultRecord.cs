
using Akka.Actor;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Mate.Production.Core.Agents.ProductionAgent.Production.Instruction;
using System;
using Mate.Production.Core.Environment.Records.Interfaces;

namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record BucketResultRecord
    (
        Guid Key,
        DateTime  CreationTime,
        DateTime Start,
        DateTime End,
        TimeSpan OriginalDuration,
        IActorRef ProductionAgent,
        string CapabilityProvider
    ) : IKey, IJobResult
    { 
        public IJobResult FinishedAt(DateTime timestamp) => 
            this with { End = timestamp };
        public IJobResult UpdateProductionAgent(IActorRef p) => 
            this with { ProductionAgent = p };
    }
}
