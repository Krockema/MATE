using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class PrtWorkcenter
    {
        public string PrtId { get; set; }
        public string WorkcenterId { get; set; }
        public long? PriorityValue { get; set; }
        public double? FactorSetupTime { get; set; }
    }
}
