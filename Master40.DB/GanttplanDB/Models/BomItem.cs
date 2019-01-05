using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class BomItem
    {
        public string BomId { get; set; }
        public string ItemId { get; set; }
        public string AlternativeId { get; set; }
        public string Name { get; set; }
        public long? Standard { get; set; }
        public string MaterialId { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
        public double? PreparationTime { get; set; }
        public string Group { get; set; }
    }
}
