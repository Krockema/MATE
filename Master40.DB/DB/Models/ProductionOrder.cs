using System.Collections.Generic;
using Master40.DB.Models;
using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class ProductionOrder : BaseEntity
    {
        public int ArticleId { get; set; }
        [JsonIgnore]
        public Article Article { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductionOrderBom> ProductionOrderBoms {get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductionOrderBom> ProdProductionOrderBomChilds { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public int Duetime { get; set; }
        [JsonIgnore]
        public virtual ICollection<DemandProviderProductionOrder> DemandProviderProductionOrders { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductionOrderWorkSchedule> ProductionOrderWorkSchedule { get; set; }
    }
}
