using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Master40.DB.Enums;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public interface IDemandToProvider
    {
        int Id { get; set; }
        int ArticleId { get; set; }
        Article Article { get; set; }
        decimal Quantity { get; set; }
        int? DemandRequesterId { get; set; }
        DemandToProvider DemandRequester { get; set; }
        List<DemandToProvider> DemandProvider { get; set; }
        State State { get; set; }

    }

    

    /// <summary>
    /// derived Class for Damand to DemandProvider
    /// To Access a specific Demand use:
    /// var purchaseDemands = context.Demands.OfType<DemandPurchase>().ToList();
    /// </summary>
    public abstract class DemandToProvider : BaseEntity, IDemandToProvider
    {
        public const string ARTICLE_FKEY = "Article";
        public int ArticleId { get; set; }
        [JsonIgnore]
        public Article Article { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        public int? DemandRequesterId { get; set; }
        public const string DEMANDRQUESTER_FKEY = "DemandRequester";

        [JsonIgnore]
        public DemandToProvider DemandRequester { get; set; }
        [JsonIgnore]
        public virtual List<DemandToProvider> DemandProvider { get; set; }

        [Required]
        public int StateId { get; set; }

        public State State { get; set; } 
        //public ICollection<ArticleToDemand> ArtilceToDemand { get; set; }
    }

    public class DemandOrderPart : DemandToProvider
    {
        public const string ORDERPART_FKEY = "OrderPart";
        public int OrderPartId { get; set; }
        [JsonIgnore]
        public OrderPart OrderPart { get; set; }
        
    }
    public class DemandProductionOrderBom : DemandToProvider
    {
        public const string PRODUCTIONORDERBOM_FKEY = "ProductionOrderBom";
        public int? ProductionOrderBomId { get; set; }
        [JsonIgnore]
        public ProductionOrderBom ProductionOrderBom { get; set; }
    }
    public class DemandStock : DemandToProvider
    {
        public const string STOCK_FKEY = "Stock";
        public int StockId { get; set; }
        [JsonIgnore]
        public Stock Stock { get; set; }
    }
    public class DemandProviderStock : DemandToProvider
    {
        public const string STOCK_FKEY = "Stock";
        public int StockId { get; set; }
        [JsonIgnore]
        public Stock Stock { get; set; }
    }
    public class DemandProviderPurchasePart : DemandToProvider
    {
        public const string PURCHASEPART_FKEY = "PurchasePart";
        public int PurchasePartId { get; set; }
        [JsonIgnore]
        public PurchasePart PurchasePart { get; set; }
    }
    public class DemandProviderProductionOrder : DemandToProvider
    {
        public const string PRODUCTIONORDER_FKEY = "ProductionOrder";
        public int ProductionOrderId { get; set; }
        [JsonIgnore]
        public ProductionOrder ProductionOrder { get; set; }
    }
}

/*
namespace Master40.DB.Models
{
    public class DemandToProviderTest : IDemand, IProvider
    {
        public int RequesterId { get; set; }
        public int ProviderId { get; set; }
        public string Source { get; set; }
    }
}
*/