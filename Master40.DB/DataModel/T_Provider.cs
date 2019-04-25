using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public abstract class T_Provider : BaseEntity, IProvider
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
    
    public class ProviderStock : T_Provider
    {
        public const string STOCK_FKEY = "Stock";
        public int StockId { get; set; }
        [JsonIgnore]
        public M_Stock Stock { get; set; }
    }
    public class ProviderPurchasePart : T_Provider
    {
        public const string PURCHASEPART_FKEY = "PurchasePart";
        public int PurchasePartId { get; set; }
        [JsonIgnore]
        public T_PurchaseOrderPart PurchasePart { get; set; }
    }
    public class ProviderProductionOrder : T_Provider
    {
        public const string PRODUCTIONORDER_FKEY = "ProductionOrder";
        public int ProductionOrderId { get; set; }
        [JsonIgnore]
        public T_ProductionOrder ProductionOrder { get; set; }
    }
}