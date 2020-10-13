using Master40.DB.Nominal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.ReportingModel.Interface
{
    public interface ISimulationResourceData : ISimulationTask
    {
        int SimulationConfigurationId { get; set; }
        SimulationType SimulationType { get; set; }
        int SimulationNumber { get; set; }
        string CapabilityProvider { get; set; }
        string CapabilityName { get; set; }
        long ExpectedDuration { get; set; }
        int SetupId { get; set; }
        long Time { get; set; }
    }
}
