using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ReportColorfilterItem
    {
        public string ReportId { get; set; }
        public string FilterId { get; set; }
        public string PropertyId { get; set; }
        public string Value { get; set; }
        public long? CaseSensitive { get; set; }
        public long? FilterMatchingType { get; set; }
    }
}
