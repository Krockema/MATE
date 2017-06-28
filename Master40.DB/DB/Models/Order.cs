using System.Collections.Generic;

namespace Master40.DB.DB.Models
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
