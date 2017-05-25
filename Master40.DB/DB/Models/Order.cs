using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.DB.Models
{
    public class Order : BaseEntity
    {
        public string Name { get; set; }
        public int DueTime { get; set; }
        public int BusinessPartnerId { get; set; }
        public BusinessPartner BusinessPartner { get; set; }
        public virtual ICollection<OrderPart> OrderParts { get; set; }
    }
}
