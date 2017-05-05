using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ProductionOrderBom
    {
        public int ProductionOrderBomId { get; set; }
        public int? ParentProductionOrderId { get; set; }
        public int ProductionOrderId { get; set; }
        public ProductionOrder ParentProductionOrder { get; set; }
        public ProductionOrder ProductionOrder { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
