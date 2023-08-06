using Mate.DataCore.DataModel;
using System;

namespace Mate.Production.Core.Environment.Records
{
    public record ResourceHubInformationRecord
    (
        M_ResourceCapability Capability,
        object WorkTimeGenerator,
        TimeSpan MaxBucketSize
    );
}

