using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class WorkcenterWorker
    {
        public string WorkcenterId { get; set; }
        public string GroupId { get; set; }
        public string ActivityqualificationId { get; set; }
        public long? ActivityType { get; set; }
        public long? ChangeWorkerType { get; set; }
        public long? WorkerRequirementCount { get; set; }
        public long? WorkerRequirementCountMax { get; set; }
        public double? WorkerRequirementUtilization { get; set; }
    }
}
