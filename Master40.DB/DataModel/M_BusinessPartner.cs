using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_BusinessPartner : BaseEntity
    {
        public string Name { get; set; }
        public bool Debitor { get; set; }
        public bool Kreditor { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_PurchaseOrder> Purchases { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_CustomerOrder> Orders { get; set; }
        [JsonIgnore]
        public virtual ICollection<M_ArticleToBusinessPartner> ArticleToBusinesspartner { get; set; }
    }
}
