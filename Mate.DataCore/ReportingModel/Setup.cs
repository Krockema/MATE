using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel.Interface;
using System;

namespace Mate.DataCore.ReportingModel
{
    public class Setup : ResultBaseEntity, ISimulationResourceData
    {
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public DateTime Time { get; set; }
        public TimeSpan ExpectedDuration { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string CapabilityProvider { get; set; }
        public string CapabilityName { get; set; }
        public int SetupId { get; set; }
        public string Mapping { get => CapabilityName; }
    }
}
