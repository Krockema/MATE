using System.Collections.Generic;
using Master40.DB.Enums;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_PurchaseOrderPart : BaseEntity
    {
        public const string ARTICLE_FKEY = "Article";
        public const string PURCHASE_FKEY = "Purchase";
        public int PurchaseId { get; set; }
        [JsonIgnore]
        public T_PurchaseOrder Purchase { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProviderPurchasePart> DemandProviderPurchaseParts { get; set; }
        public State State { get; set; }
    }
}
