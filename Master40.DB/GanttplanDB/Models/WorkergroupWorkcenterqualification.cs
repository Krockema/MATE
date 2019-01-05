using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class WorkergroupWorkcenterqualification
    {
        public string WorkergroupId { get; set; }
        public string WorkcenterId { get; set; }
        public string ValidFrom { get; set; }
        public long WorkcenterqualificationType { get; set; }
        public long? PriorityValue { get; set; }
        public string ValidUntil { get; set; }
    }
}
