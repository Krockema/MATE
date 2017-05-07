using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ProductionOrderBom
    {
        public int ProductionOrderBomId { get; set; }
        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; }
        public ICollection<ProductionOrderBomItem> ProductionOrderBomItems { get; set; }
        public string Name { get; set; }
    }
}
