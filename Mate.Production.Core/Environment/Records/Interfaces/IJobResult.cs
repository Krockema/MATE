using System;

namespace Mate.Production.Core.Environment.Records.Interfaces
{
    public interface IJobResult
    {
        Guid Key { get; }
        DateTime Start { get; }
        DateTime End { get; }
        string CapabilityProvider { get; }
        TimeSpan OriginalDuration { get; }
        IJobResult FinishedAt(DateTime dateTime);
    }
}