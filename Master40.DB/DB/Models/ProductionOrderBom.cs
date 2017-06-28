using System.Collections.Generic;

namespace Master40.DB.DB.Models
{
    public class ProductionOrderBom : BaseEntity
    {
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
