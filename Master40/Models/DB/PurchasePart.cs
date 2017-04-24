using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class PurchasePart
    {
        public int PurchasePartId { get; set; }
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public int Amount { get; set; }
    }
}
