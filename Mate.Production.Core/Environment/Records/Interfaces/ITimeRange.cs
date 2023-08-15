using System;

namespace Mate.Production.Core.Environment.Records.Interfaces
{
    public interface ITimeRange
    {
        DateTime Start { get; }
        DateTime End { get; }
    }
}
