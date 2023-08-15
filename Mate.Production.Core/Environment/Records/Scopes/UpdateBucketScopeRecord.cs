using System;

namespace Mate.Production.Core.Environment.Records.Scopes
{
    public record UpdateBucketScopeRecord
    (
        Guid BucketKey,
        TimeSpan Duration
    );
}

