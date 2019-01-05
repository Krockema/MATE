using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class UnitUnitconversion
    {
        public string UnitId { get; set; }
        public string ConversionUnitId { get; set; }
        public double? ConversionFactor { get; set; }
    }
}
