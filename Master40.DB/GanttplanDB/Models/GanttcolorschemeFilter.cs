using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class GanttcolorschemeFilter
    {
        public string GanttcolorschemeId { get; set; }
        public string FilterId { get; set; }
        public long? Color { get; set; }
        public long? PriorityValue { get; set; }
    }
}
