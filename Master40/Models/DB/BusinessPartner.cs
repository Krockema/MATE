using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class BusinessPartner
    {
        public int BusinessPartnerId { get; set; }
        public string Name { get; set; }
        public bool Debitor { get; set; }
        public bool Kreditor { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
