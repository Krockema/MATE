using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Report
    {
        public string ReportId { get; set; }
        public string Name { get; set; }
        public string SortingColumn { get; set; }
        public string SelectedObjectId { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public long? ReportType { get; set; }
        public long? FixedColumnsCount { get; set; }
        public long? OrderAsc { get; set; }
        public long? PagebreakAfterObject { get; set; }
        public string UserId { get; set; }
        public string Category { get; set; }
    }
}
