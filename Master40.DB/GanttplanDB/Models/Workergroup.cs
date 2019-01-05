using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Workergroup
    {
        public string WorkergroupId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public long? PoolType { get; set; }
        public double? SetupTimePenalty { get; set; }
        public double? CostRate { get; set; }
        public double? ProcessingTimePenalty { get; set; }
        public long? CapacityType { get; set; }
        public double? AllocationMax { get; set; }
    }
}
