using System;
using System.ComponentModel.DataAnnotations.Schema;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel.Interface;

namespace Mate.DataCore.ReportingModel
{
    public class Job : ResultBaseEntity, ISimulationResourceData
    {
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public DateTime Time { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string Article { get; set; }
        public TimeSpan ExpectedDuration { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime ReadyAt { get; set; }
        public string CapabilityProvider { get; set; }
        public string CapabilityName { get; set; }
        public string JobId { get; set; }
        public string ProductionOrderId { get; set; }
        public DateTime DueTime { get; set; }
        public string OrderId { get; set; }
        public string CreatedForOrderId { get; set; }
        public int HierarchyNumber { get; set; }
        public string Parent { get; set; }
        public string ParentId { get; set; }
        public string Bucket { get; set; }
        public int SetupId { get; set; }
        [NotMapped]
        public string ArticleKey { get; set; }
        [NotMapped]
        public string ArticleType { get; set; }
        [NotMapped]
        public string Mapping { get => CapabilityName; }

    }
}