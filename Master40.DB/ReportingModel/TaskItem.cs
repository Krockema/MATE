using Master40.DB.Nominal;
using Master40.DB.Nominal.Model;
using Master40.DB.ReportingModel.Interface;

namespace Master40.DB.ReportingModel
{
    public class TaskItem : ResultBaseEntity, ISimulationTask
    {
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public string Type { get; set; } //Processing or Setup
        public string Resource { get; set; }
        public ResourceType ResourceType { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string CapabilityName { get; set; }
        public string Operation { get; set; } //i.e. Cut Baseplate or Setup Tool XX
        public string GroupId { get; set; } //usually the bucket
        public string Mapping { get  => Resource; }
    }
}
