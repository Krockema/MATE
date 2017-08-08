using System.Collections.Generic;
using Master40.DB.Enums;
using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class PurchasePart : BaseEntity
    {
        public int PurchaseId { get; set; }
        [JsonIgnore]
        public Purchase Purchase { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public Article Article { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public virtual ICollection<DemandProviderPurchasePart> DemandProviderPurchaseParts { get; set; }
        public State State { get; set; }
    }
}
