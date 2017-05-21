using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Master40.DB.Models
{
    public class ProductionOrder : BaseEntity
    {
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public virtual ICollection<ProductionOrderBom> ProductionOrderBoms {get; set; }
        public virtual ICollection<ProductionOrderBom> ProdProductionOrderBomChilds { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public virtual ICollection<DemandProviderProductionOrder> DemandProviderProductionOrders { get; set; }
        public virtual ICollection<ProductionOrderWorkSchedule> ProductionOrderWorkSchedule { get; set; }
    }
}
