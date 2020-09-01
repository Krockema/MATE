using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.Interfaces;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblPrt : IGptblResource
    {
        public string ClientId { get; set; }
        public string PrtId { get; set; }
        [NotMapped]
        public string Id { get => PrtId; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public double? AllocationMax { get; set; }
        public int? AllowChangeWorkcenter { get; set; }
        public int? CapacityType { get; set; }
        public double? FactorProcessingSpeed { get; set; }
        public int? MaintenanceIntervalQuantity { get; set; }
        public string MaintenanceIntervalQuantityUnitId { get; set; }
        public int? MaintenanceIntervalTime { get; set; }
        public int? SetupTime { get; set; }
        public DateTime? LastModified { get; set; }
        public int? InterruptionTimeMax { get; set; }
        public int? ParallelAllocationCriteria { get; set; }
        public int? SynchronousStart { get; set; }
        public double? CostRateSetup { get; set; }
        public double? CostRateProcessing { get; set; }
        public string GlobalCalendarId { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
