using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblChart
    {
        public string ClientId { get; set; }
        public string ChartId { get; set; }
        public string Name { get; set; }
        public int? AllInOne { get; set; }
        public string ArgumentDataMemberId { get; set; }
        public string ChartType { get; set; }
        public string ColorPaletteName { get; set; }
        public int? CustomChartTitle { get; set; }
        public int? CustomXaxisTitle { get; set; }
        public int? CustomYaxisTitle { get; set; }
        public int? HeaderAsArgument { get; set; }
        public string ReportId { get; set; }
        public int? ReportType { get; set; }
        public int? ShowLabels { get; set; }
        public int? ShowLegend { get; set; }
        public int? Rotated { get; set; }
        public string UserId { get; set; }
        public string Category { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
