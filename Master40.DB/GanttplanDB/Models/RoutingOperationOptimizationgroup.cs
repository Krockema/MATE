using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class RoutingOperationOptimizationgroup
    {
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public long SplitId { get; set; }
        public long OptimizationgroupType { get; set; }
        public string OptimizationgroupId { get; set; }
        public double? OptimizationgroupValue { get; set; }
    }
}
