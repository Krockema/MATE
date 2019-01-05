using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ChartTextItem
    {
        public string ChartId { get; set; }
        public string TextId { get; set; }
        public string PropertyId { get; set; }
        public string PropertyName { get; set; }
        public long? PropertyNameType { get; set; }
    }
}
