using System.Collections.Generic;
using Master40.DB.Enums;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class OrderPart : BaseEntity
    {
        public const string ORDER_FKEY = "Order";
        public const string ARTICLE_FKEY = "Article";
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public Article Article { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public virtual ICollection<DemandOrderPart> DemandOrderParts { get; set; }
        public bool IsPlanned { get; set; }
        public State State { get; set; }
        
        /*
        [NotMapped]
        public int RequesterId { get => this.OrderPartId; }
        public DemandToProvider DemandToDemand { get; set; }
        [NotMapped]
        public string Source { get; private set; }
        */
    }

}


