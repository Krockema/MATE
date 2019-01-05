using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ReportColumn
    {
        public string ReportId { get; set; }
        public long Index { get; set; }
        public string PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string Formula { get; set; }
    }
}
