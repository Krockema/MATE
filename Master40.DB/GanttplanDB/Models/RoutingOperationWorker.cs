using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class RoutingOperationWorker
    {
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public long SplitId { get; set; }
        public string GroupId { get; set; }
        public string ActivityqualificationId { get; set; }
        public long? ActivityType { get; set; }
        public long? ChangeWorkerType { get; set; }
        public string WorkcenterId { get; set; }
        public long? WorkerRequirementCount { get; set; }
        public long? WorkerRequirementCountMax { get; set; }
        public double? WorkerRequirementUtilization { get; set; }
    }
}
