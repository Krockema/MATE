using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class DemandPurchase
    {
        [Key]
        public int DemandPurchseId { get; set; }
        public int DemandId { get; set; }
        public int PurchasePartId { get; set; }
        public Demand Demand { get; set; }
        public PurchasePart PurchasePart { get; set; }
        public int Quantity { get; set; }
    }
}
