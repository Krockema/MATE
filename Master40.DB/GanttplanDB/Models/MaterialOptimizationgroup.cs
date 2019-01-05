using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class MaterialOptimizationgroup
    {
        public string MaterialId { get; set; }
        public long OptimizationgroupType { get; set; }
        public string OptimizationgroupId { get; set; }
        public double? OptimizationgroupValue { get; set; }
    }
}
