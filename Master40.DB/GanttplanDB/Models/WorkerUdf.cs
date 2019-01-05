using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class WorkerUdf
    {
        public string WorkerId { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
