using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblReport
    {
        public string ClientId { get; set; }
        public string ReportId { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public int? FixedColumnsCount { get; set; }
        public string Name { get; set; }
        public int? OrderAsc { get; set; }
        public int? PagebreakAfterObject { get; set; }
        public int? ReportType { get; set; }
        public string SelectedObjectId { get; set; }
        public string SortingColumn { get; set; }
        public string UserId { get; set; }
        public string Category { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
