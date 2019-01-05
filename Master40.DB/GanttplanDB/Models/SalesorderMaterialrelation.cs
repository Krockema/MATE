using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class SalesorderMaterialrelation
    {
        public string SalesorderId { get; set; }
        public string ChildId { get; set; }
        public long MaterialrelationType { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
        public string InfoDateAvailability { get; set; }
        public double? InfoTimeBuffer { get; set; }
        public long? Fixed { get; set; }
    }
}
