using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class Order
    {
        public int OrderId { get; set; }
        public string Name { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public int BusinessPartnerId { get; set; }
        public BusinessPartner BusinessPartner { get; set; }
        public virtual ICollection<OrderPart> OrderParts { get; set; }
    }
}
