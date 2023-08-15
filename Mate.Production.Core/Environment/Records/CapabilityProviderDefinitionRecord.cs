using Mate.DataCore.Nominal.Model;
using System;

namespace Mate.Production.Core.Environment.Records
{
    public record CapabilityProviderDefinitionRecord
    (
        object WorkTimeGenerator,
        object Resource,
        ResourceType ResourceType,
        object CapabilityProvider,
        TimeSpan MaxBucketSize,
        TimeSpan TimeConstraintQueueLength,
        bool Debug
    );
}
