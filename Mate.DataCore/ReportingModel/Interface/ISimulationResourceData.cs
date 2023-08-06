using Mate.DataCore.Nominal;
using System;

namespace Mate.DataCore.ReportingModel.Interface
{
    public interface ISimulationResourceData : ISimulationTask
    {
        int SimulationConfigurationId { get; set; }
        SimulationType SimulationType { get; set; }
        int SimulationNumber { get; set; }
        string CapabilityProvider { get; set; }
        string CapabilityName { get; set; }
        TimeSpan ExpectedDuration { get; set; }
        int SetupId { get; set; }
        DateTime Time { get; set; }
    }
}
