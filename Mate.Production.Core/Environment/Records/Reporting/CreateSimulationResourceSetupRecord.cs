using System;

namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record CreateSimulationResourceSetupRecord
    (
        DateTime Start,
        TimeSpan Duration,
        string CapabilityProvider,
        string CapabilityName,
        TimeSpan ExpectedDuration,
        int SetupId
    );
}

