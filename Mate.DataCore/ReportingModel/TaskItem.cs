using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.DataCore.ReportingModel.Interface;
using System;

namespace Mate.DataCore.ReportingModel
{
    public class TaskItem : ResultBaseEntity, ISimulationTask
    {
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public string Type { get; set; } //Processing or Setup
        public string Resource { get; set; }
        public ResourceType ResourceType { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string CapabilityName { get; set; }
        public string Operation { get; set; } //i.e. Cut Baseplate or Setup Tool XX
        public long GroupId { get; set; } //usually the bucket
        public string Mapping { get  => Resource; }
    }
}
