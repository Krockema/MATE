using System.Collections.Generic;
using Master40.DB.DataModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Master40.DB;
using Master40.DB.Enums;
using Newtonsoft.Json;


namespace Zpp.ModelExtensions
{
    public abstract class Demand : BaseEntity, IDemandToProvider
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