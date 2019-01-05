using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ReportColorfilter
    {
        public string ReportId { get; set; }
        public string FilterId { get; set; }
        public long? FilterOperator { get; set; }
        public long? FilterResultCountMax { get; set; }
        public string PropertyId { get; set; }
        public long? FullRowColoring { get; set; }
        public long? FullColumnColoring { get; set; }
        public long? BgColor { get; set; }
        public long? FontColor { get; set; }
        public long? PriorityValue { get; set; }
    }
}
