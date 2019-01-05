using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Stockquantityposting
    {
        public string StockquantitypostingId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string MaterialId { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
        public long? PostingType { get; set; }
        public string Date { get; set; }
        public string InfoObjecttypeId { get; set; }
        public string InfoObjectId { get; set; }
    }
}
