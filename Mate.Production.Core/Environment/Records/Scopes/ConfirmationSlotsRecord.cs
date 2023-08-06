using Mate.Production.Core.Environment.Records.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.Core.Environment.Records.Scopes
{
    public record ConfirmationSlotsRecord
    
        (   bool IsQueueAble,
            bool IsRequieringSetup,
            IImmutableSet<ITimeRange> Scopes
    ){
        public DateTime GetScopeStart() => Scopes.Min(y => y.Start);
        public DateTime GetScopeEnd() => Scopes.Max(y => y.End);
    }
}
