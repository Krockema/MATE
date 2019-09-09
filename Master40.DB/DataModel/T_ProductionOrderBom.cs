using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_ProductionOrderBom : BaseEntity, IDemand
    {
        public int ProductionOrderParentId { get; set; }
        [JsonIgnore]
        public T_ProductionOrder ProductionOrderParent { get; set; }
        [JsonIgnore]
        public decimal Quantity { get; set; }
        public int? ProductionOrderOperationId { get; set; }
        public T_ProductionOrderOperation ProductionOrderOperation { get; set; }
        public int ArticleChildId { get; set; }
        [JsonIgnore]
        public M_Article ArticleChild { get; set; }
        
        public Quantity GetQuantity()
        {
            return new Quantity(Quantity);
        }

        public override string ToString()
        {
            return $"{ArticleChild.Name}: {ProductionOrderOperation}";
        }
    }
}
