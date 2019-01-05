using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Chart
    {
        public string ChartId { get; set; }
        public string Name { get; set; }
        public string ReportId { get; set; }
        public long? ReportType { get; set; }
        public string ArgumentDataMemberId { get; set; }
        public string ChartType { get; set; }
        public long? AllInOne { get; set; }
        public string ColorPaletteName { get; set; }
        public long? ShowLegend { get; set; }
        public long? ShowLabels { get; set; }
        public long? Rotated { get; set; }
        public long? CustomChartTitle { get; set; }
        public long? CustomXaxisTitle { get; set; }
        public long? CustomYaxisTitle { get; set; }
        public long? HeaderAsArgument { get; set; }
        public string UserId { get; set; }
        public string Category { get; set; }
    }
}
