using Mate.Production.Core.Environment.Records.Interfaces;
using System;
namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record JobResultRecord(Guid Key
        , DateTime Start
        , DateTime End
        , string CapabilityProvider
        , TimeSpan OriginalDuration) : IJobResult
    {
        public IJobResult FinishedAt(DateTime timestamp) => 
            this with {End = timestamp
                      ,OriginalDuration = this.End - this.Start};
    }
}
