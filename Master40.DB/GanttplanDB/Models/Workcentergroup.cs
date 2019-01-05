using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Workcentergroup
    {
        public string WorkcentergroupId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public double? IdleTimePeriod { get; set; }
        public long? CapacityType { get; set; }
        public double? AllocationMax { get; set; }
        public long? ParallelSchedulingType { get; set; }
        public long? ParallelAllocationCriteria { get; set; }
        public long? MandatoryTimeInterval { get; set; }
        public long? LineType { get; set; }
    }
}
