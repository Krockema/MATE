using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Master40.DB.Models
{
    public class OrderPart : BaseEntity
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public int Quantity { get; set; }
        public virtual ICollection<DemandOrderPart> DemandOrderParts { get; set; }
        public bool IsPlanned { get; set; }
        
        /*
        [NotMapped]
        public int RequesterId { get => this.OrderPartId; }
        public DemandToProvider DemandToDemand { get; set; }
        [NotMapped]
        public string Source { get; private set; }
        */
    }

}

