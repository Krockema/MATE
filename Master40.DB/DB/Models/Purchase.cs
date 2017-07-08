using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class Purchase : BaseEntity
    {
        public string Name { get; set; }
        public int DueTime { get; set; }
        public int BusinessPartnerId { get; set; }
        [JsonIgnore]
        public BusinessPartner BusinessPartner { get; set; }
        [JsonIgnore]
        public virtual ICollection<PurchasePart> PurchaseParts { get; set; }
    }
}
