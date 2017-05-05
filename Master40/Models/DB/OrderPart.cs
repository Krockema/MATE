using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class OrderPart
    {
        public int OrderPartId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public int Quantity { get; set; }
        public ICollection<DemandOrder> DemandOdrders { get; set; }
    }

}

