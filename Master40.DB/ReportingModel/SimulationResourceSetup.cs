using Master40.DB.Enums;
using Master40.DB.ReportingModel.Interface;

namespace Master40.DB.ReportingModel
{
    public class SimulationResourceSetup : BaseEntity, ISimulationResourceData
    {
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public long Time { get; set; }
        public long ExpectedDuration { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string Resource { get; set; }
        public string ResourceTool { get; set; }
    }
}
