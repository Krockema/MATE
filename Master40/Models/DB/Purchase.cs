using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public string Name { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public int BusinessPartnerId { get; set; }
        public BusinessPartner BusinessPartner { get; set; }
        public virtual ICollection<PurchasePart> PurchaseParts { get; set; }
    }
}
