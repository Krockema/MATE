using System.Collections.Generic;
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
        public State State { get; set; }
        public int DemandID { get; set; }
        public T_Demand Demand { get; set; }
        public int? ProductionOrderOperationId { get; set; }
        public T_ProductionOrderOperation ProductionOrderOperation { get; set; }
        public int ArticleChildId { get; set; }
        [JsonIgnore]
        public M_Article ArticleChild { get; set; }

        public T_ProductionOrderBom()
        {
            // it must be always a T_Demand created for every IDemand
            Demand = new T_Demand();
        }
        
        public int GetDueTime()
        {
            return ProductionOrderParent.DueTime;
        }
        
        public M_Article GetArticle()
        {
            return ArticleChild;
        }

        public decimal GetQuantity()
        {
            return Quantity;
        }
    }
}
