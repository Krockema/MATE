using System;

namespace Mate.Production.Core.Environment.Records
{
    public record UpdateSimulationJobRecord(
        IJob Job,
        string JobType,
        TimeSpan Duration,
        DateTime Start,
        string CapabilityProvider,
        string Capability,
        DateTime ReadyAt,
        string Bucket,
        int SetupId
    );
}