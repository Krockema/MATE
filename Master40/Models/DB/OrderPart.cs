using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class OrderPart //: IDemand
    {
        [Key]
        public int OrderPartId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public int Quantity { get; set; }
        public virtual ICollection<DemandOrderPart> DemandOdrderParts { get; set; }
        /*
        [NotMapped]
        public int RequesterId { get => this.OrderPartId; }
        public DemandToProvider DemandToDemand { get; set; }
        [NotMapped]
        public string Source { get; private set; }
        */
    }

}

