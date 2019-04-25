using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_Demand : BaseEntity, IDemand
    {
        public const string ARTICLE_FKEY = "Article";
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        public int? DemandRequesterId { get; set; }
        public const string DEMANDRQUESTER_FKEY = "DemandRequester";

        [JsonIgnore]
        public T_DemandToProvider DemandRequester { get; set; }
        [JsonIgnore]
        public virtual List<T_DemandToProvider> DemandProvider { get; set; }

        [Required]
        public int StateId { get; set; }

        public State State { get; set; } 
        //public ICollection<ArticleToDemand> ArtilceToDemand { get; set; }
    }
    
    public class DemandCustomerOrderPart : T_Demand
    {
        public const string ORDERPART_FKEY = "OrderPart";
        public int OrderPartId { get; set; }
        [JsonIgnore]
        public T_CustomerOrderPart OrderPart { get; set; }
        
    }
    public class DemandProductionOrderBom : T_Demand
    {
        public const string PRODUCTIONORDERBOM_FKEY = "ProductionOrderBom";
        public int? ProductionOrderBomId { get; set; }
        [JsonIgnore]
        public T_ProductionOrderBom ProductionOrderBom { get; set; }
    }
    public class DemandStock : T_Demand
    {
        public const string STOCK_FKEY = "Stock";
        public int StockId { get; set; }
        [JsonIgnore]
        public M_Stock Stock { get; set; }
    }
}