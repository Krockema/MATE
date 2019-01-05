using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ReportRowfilter
    {
        public string ReportId { get; set; }
        public string FilterId { get; set; }
        public long? FilterOperator { get; set; }
        public long? FilterResultCountMax { get; set; }
        public long? PriorityValue { get; set; }
    }
}
