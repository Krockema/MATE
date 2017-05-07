using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ProductionOrderBomItem
    {
        public int ProductionOrderBomItemId { get; set; }
        public int ProductionOrderId { get; set; }
        public int ProductionOderBomId { get; set; }
        public ProductionOrder ProductionOrder { get; set; }
        public ProductionOrderBom ProductionOrderBom { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
