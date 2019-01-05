using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class MaterialUnitconversion
    {
        public string MaterialId { get; set; }
        public string UnitId { get; set; }
        public string ConversionUnitId { get; set; }
        public double? ConversionFactor { get; set; }
    }
}
