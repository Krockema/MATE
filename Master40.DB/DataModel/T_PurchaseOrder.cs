using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_PurchaseOrder : BaseEntity
    {
        public const string BUSINESSPARTNER_FKEY = "BusinessPartner";
        public string Name { get; set; }
        public int DueTime { get; set; }
        public int BusinessPartnerId { get; set; }
        [JsonIgnore]
        public M_BusinessPartner BusinessPartner { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_PurchaseOrderPart> PurchaseParts { get; set; }
    }
}
