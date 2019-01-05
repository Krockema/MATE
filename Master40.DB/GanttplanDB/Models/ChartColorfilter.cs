using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ChartColorfilter
    {
        public string ChartId { get; set; }
        public string FilterId { get; set; }
        public long? FilterOperator { get; set; }
        public string PropertyId { get; set; }
        public long? Color { get; set; }
        public long? PriorityValue { get; set; }
    }
}
