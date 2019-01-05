using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class RoutingOperation
    {
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public long SplitId { get; set; }
        public long? SplitMin { get; set; }
        public long? SplitMax { get; set; }
        public long? OperationType { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? AllowInterruptions { get; set; }
        public long? WorkerRequirementType { get; set; }
    }
}
