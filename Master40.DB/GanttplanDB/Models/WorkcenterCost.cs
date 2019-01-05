using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class WorkcenterCost
    {
        public string WorkcenterId { get; set; }
        public string ValidFrom { get; set; }
        public double? CostRateProcessing { get; set; }
        public double? CostRateSetup { get; set; }
        public double? CostRateIdleTime { get; set; }
    }
}
