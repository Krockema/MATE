using System;
namespace Mate.Production.Core.Environment.Records.Reporting
{
    public record ComputationalTimerRecord(
        DateTime Time,
        string Timertype,
        TimeSpan Duration
    );
}