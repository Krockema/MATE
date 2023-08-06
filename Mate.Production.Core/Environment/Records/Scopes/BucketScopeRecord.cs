using Mate.Production.Core.Environment.Records.Interfaces;
using System;

namespace Mate.Production.Core.Environment.Records.Scopes
{
    public record BucketScopeRecord(
        Guid BucketKey,
        DateTime Start,
        DateTime End,
        TimeSpan Duration
    ) : ITimeRange;
}
