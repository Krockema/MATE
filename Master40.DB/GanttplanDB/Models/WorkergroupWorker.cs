using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class WorkergroupWorker
    {
        public string WorkergroupId { get; set; }
        public string WorkerId { get; set; }
        public long? GroupType { get; set; }
    }
}
