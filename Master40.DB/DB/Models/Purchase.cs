using System.Collections.Generic;

namespace Master40.DB.DB.Models
{
    public class Purchase : BaseEntity
    {
        public string Name { get; set; }
        public int DueTime { get; set; }
        public int BusinessPartnerId { get; set; }
        public BusinessPartner BusinessPartner { get; set; }
        public virtual ICollection<PurchasePart> PurchaseParts { get; set; }
    }
}
