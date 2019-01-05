using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class PriorityLatenesscost
    {
        public string PriorityId { get; set; }
        public long Start { get; set; }
        public string Name { get; set; }
        public double? CostsAbsolute { get; set; }
        public double? CostsRelative { get; set; }
        public long? CostsAbsoluteInterval { get; set; }
    }
}
