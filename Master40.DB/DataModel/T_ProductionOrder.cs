using System.Collections.Generic;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_ProductionOrder : BaseEntity, IProvider
    {
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderBom> ProductionOrderBoms {get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderBom> ProdProductionOrderBomChilds { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public int DueTime { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderOperation> ProductionOrderWorkSchedule { get; set; }

        public int ProviderId { get; set; }
        public T_Provider Provider { get; set; }
        
        public int GetAvailabilityTime()
        {
            return DueTime;
        }
        
        public M_Article GetArticle()
        {
            return Article;
        }
    }
}
