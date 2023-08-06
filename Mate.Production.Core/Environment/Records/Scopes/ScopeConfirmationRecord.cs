using Mate.Production.Core.Environment.Records.Interfaces;
using System;
using System.Collections.Immutable;
using System.Linq;


namespace Mate.Production.Core.Environment.Records.Scopes
{
    public record ScopeConfirmationRecord(

            ImmutableHashSet<ITimeRange> Scopes,
            DateTime SetReadyAt
)
    {
        public DateTime GetScopeStart() => this.Scopes.Min(y => y.Start);
        public DateTime GetScopeEnd() => this.Scopes.Max(y => y.End);
        public ITimeRange GetSetup() => this.Scopes.SingleOrDefault(y => y.GetType().Equals(typeof(SetupSlotRecord)));
        public ITimeRange GetProcessing() => this.Scopes.SingleOrDefault(y => y.GetType().Equals(typeof(ProcessingSlotRecord)));
    }
}
