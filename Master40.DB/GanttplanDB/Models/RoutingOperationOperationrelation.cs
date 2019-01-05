using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class RoutingOperationOperationrelation
    {
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public long SplitId { get; set; }
        public string SuccessorOperationId { get; set; }
        public string SuccessorAlternativeId { get; set; }
        public long SuccessorSplitId { get; set; }
        public double? OverlapValue { get; set; }
        public long? OverlapType { get; set; }
        public long? TimeBufferMin { get; set; }
        public long? TimeBufferMax { get; set; }
    }
}
