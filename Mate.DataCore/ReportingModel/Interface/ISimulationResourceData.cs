using Mate.DataCore.Nominal;

namespace Mate.DataCore.ReportingModel.Interface
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
