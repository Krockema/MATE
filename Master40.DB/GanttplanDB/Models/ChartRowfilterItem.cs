using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ChartRowfilterItem
    {
        public string ChartId { get; set; }
        public string FilterId { get; set; }
        public string PropertyId { get; set; }
        public string Value { get; set; }
        public long? FilterMatchingType { get; set; }
    }
}
