using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_ProductionOrder : BaseEntity
    {
        public const string ARTICLE_FKEY = "Article";
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderBom> ProductionOrderBoms {get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderBom> ProdProductionOrderBomChilds { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public int Duetime { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProviderProductionOrder> DemandProviderProductionOrders { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderOperation> ProductionOrderWorkSchedule { get; set; }
    }
}
