using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Stock
    {
        public string StockId { get; set; }
        public double? InfoStockAvg { get; set; }
        public double? InfoStockInitial { get; set; }
        public string InfoStockInitialDate { get; set; }
        public double? InfoStockMax { get; set; }
        public double? InfoStockMin { get; set; }
        public double? InfoStockFinal { get; set; }
    }
}
