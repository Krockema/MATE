using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class RoutingOperationActivity
    {
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public long SplitId { get; set; }
        public long ActivityId { get; set; }
        public long? ActivityType { get; set; }
        public double? WorkcenterAllocation { get; set; }
        public long? ParallelCountMax { get; set; }
        public double? TimeQuantityDependent { get; set; }
        public long? TimeQuantityIndependent { get; set; }
        public string Description { get; set; }
        public long? ConfirmationRequired { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public long? InterruptionTimeMax { get; set; }
    }
}
