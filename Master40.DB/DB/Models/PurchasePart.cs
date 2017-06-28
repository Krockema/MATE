using System.Collections.Generic;

namespace Master40.DB.DB.Models
{
    public class PurchasePart : BaseEntity
    {
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public int Quantity { get; set; }
        public virtual ICollection<DemandProviderPurchasePart> DemandProviderPurchaseParts { get; set; }
    }
}
