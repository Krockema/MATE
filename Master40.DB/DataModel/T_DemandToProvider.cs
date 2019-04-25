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
        M_Article Article { get; set; }
        decimal Quantity { get; set; }
        int? DemandRequesterId { get; set; }
        T_DemandToProvider DemandRequester { get; set; }
        List<T_DemandToProvider> DemandProvider { get; set; }
        State State { get; set; }

    }

    

    /// <summary>
    /// derived Class for Damand to DemandProvider
    /// To Access a specific Demand use:
    /// var purchaseDemands = context.Demands.OfType<DemandPurchase>().ToList();
    /// </summary>
    public class T_DemandToProvider : BaseEntity, IDemandToProvider
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