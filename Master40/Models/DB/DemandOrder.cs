using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class DemandOrder
    {
        [Key]
        public int DemandOrderId { get; set; }
        public int DemandId { get; set; }
        public int OrderPartId { get; set; }
        public Demand Demand { get; set; }
        public OrderPart OrderPart { get; set; }
        public int Quantity { get; set; }
    }
}
