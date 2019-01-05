using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class SetupmatrixItem
    {
        public string SetupmatrixId { get; set; }
        public string FromOptimizationgroupId { get; set; }
        public string ToOptimizationgroupId { get; set; }
        public double? SetupTime { get; set; }
    }
}
