using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mate.DataCore.DataModel
{
    public class T_PurchaseOrder : BaseEntity
    {
        public string Name { get; set; }
        public DateTime DueTime { get; set; }
        public int BusinessPartnerId { get; set; }
        [JsonIgnore]
        public M_BusinessPartner BusinessPartner { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_PurchaseOrderPart> PurchaseOrderParts { get; set; }
    }
}
