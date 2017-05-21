using System.Collections.Generic;

namespace Master40.DB.Models
{
    public class BusinessPartner : BaseEntity
    {
        public string Name { get; set; }
        public bool Debitor { get; set; }
        public bool Kreditor { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ArticleToBusinessPartner> ArticleToBusinesspartner { get; set; }
    }
}
