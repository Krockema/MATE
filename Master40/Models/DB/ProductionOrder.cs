using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ProductionOrder
    {
        [Key]
        public int ProductionOrderId { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public ICollection<ProductionOrderBom> ProductionOrderBoms {get; set; }
        public ICollection<ProductionOrderBomItem> ProductionOrderBomItems { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public ICollection<ProductionOrderToProductionOrderWorkSchedule> ProductionOrderToProductionOrderWorkSchedule { get; set; }
    }
}
