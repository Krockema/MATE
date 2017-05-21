using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ProductionOrderBom
    {
        public int ProductionOrderBomId { get; set; }


        public int? ProductionOrderParentId { get; set; }
        public ProductionOrder ProductionOrderParent { get; set; }
        public int ProductionOrderChildId { get; set; }
        public ProductionOrder ProductionOrderChild { get; set; }
        public virtual ICollection<DemandProductionOrderBom> DemandProductionOrderBoms { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
