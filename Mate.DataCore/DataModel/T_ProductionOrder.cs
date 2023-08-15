using System;
using System.Collections.Generic;
using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Interfaces;
using Newtonsoft.Json;

namespace Mate.DataCore.DataModel
{
    public class T_ProductionOrder : BaseEntity, IProvider
    {
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderBom> ProductionOrderBoms {get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderBom> ProductionOrderBomChilds { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public DateTime DueTime { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderOperation> ProductionOrderOperations { get; set; }

        public M_Article GetArticle()
        {
            return Article;
        }

        public Quantity GetQuantity()
        {
            return new Quantity(Quantity);
        }
    }
}
