using Master40.DB.Nominal;

namespace Master40.DB.ReportingModel
{
    public class TaskItem : ResultBaseEntity
    {
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public string Type { get; set; } //Processing or Setup
        public string Resource { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string Capability { get; set; }
        public string Operation { get; set; } //i.e. Cut Baseplate or Setup Tool XX
        public string GroupId { get; set; } //usually the bucket

    }
}
