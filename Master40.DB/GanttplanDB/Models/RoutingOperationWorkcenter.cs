using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class RoutingOperationWorkcenter
    {
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public long SplitId { get; set; }
        public string WorkcenterId { get; set; }
        public double? Delta { get; set; }
        public double? LotSizeMax { get; set; }
        public double? LotSizeMin { get; set; }
    }
}
