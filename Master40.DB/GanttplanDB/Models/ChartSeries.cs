using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ChartSeries
    {
        public string ChartId { get; set; }
        public string SeriesId { get; set; }
        public string ReportColumnName { get; set; }
        public string ReportColumnPropertyId { get; set; }
        public string ChartType { get; set; }
        public long? Color { get; set; }
    }
}
