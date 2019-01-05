using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class WorkcentergroupWorkcenter
    {
        public string WorkcentergroupId { get; set; }
        public string WorkcenterId { get; set; }
        public long? GroupType { get; set; }
    }
}
