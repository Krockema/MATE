using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class Demand
    {
        [Key]
        public int DemandId { get; set; }
        public int ArticleToDemandId { get; set; }
        public int Quantity { get; set; }
        public ICollection<ArticleToDemand> ArtilceToDemand { get; set; }
        public DemandPurchase DemandPurchases { get; set; }
        public DemandStock DemandStocks { get; set; }
        public DemandOrder DemandOrders { get; set; }
    }
}
