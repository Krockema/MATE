using System;

namespace Mate.Production.Core.Environment.Records
{
    public record BreakDownRecord
    (string Resource,
      string ResourceCapability,
      bool IsBroken,
      TimeSpan Duration
    )
    {
        public BreakDownRecord SetIsBroken(bool s) => this with { IsBroken = s };

    }
}
