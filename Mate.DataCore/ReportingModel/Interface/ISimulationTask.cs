using Akka.Hive.Definitions;
using System;

namespace Mate.DataCore.ReportingModel.Interface
{

    public interface ISimulationTask
    {
        DateTime Start { get; set; }
        DateTime End { get; set; }
        string Mapping { get; }
    }
}

