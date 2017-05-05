using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class DemandStock
    {
        public int DemandStockId { get; set; }
        public int DemandId { get; set; }
        public int StockId { get; set; }
        public Demand Demand { get; set; }
        public Stock Stock { get; set; }
        public int Quantity { get; set; }
    }
}
