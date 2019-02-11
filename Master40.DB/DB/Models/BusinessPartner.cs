using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class BusinessPartner : BaseEntity
    {
        public string Name { get; set; }
        public bool Debitor { get; set; }
        public bool Kreditor { get; set; }
        [JsonIgnore]
        public virtual ICollection<Purchase> Purchases { get; set; }
        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; }
        [JsonIgnore]
        public virtual ICollection<ArticleToBusinessPartner> ArticleToBusinesspartner { get; set; }
    }
}
