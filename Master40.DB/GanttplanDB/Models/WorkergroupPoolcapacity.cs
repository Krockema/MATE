using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class WorkergroupPoolcapacity
    {
        public string WorkergroupId { get; set; }
        public string DateFrom { get; set; }
        public long? Quantity { get; set; }
    }
}
