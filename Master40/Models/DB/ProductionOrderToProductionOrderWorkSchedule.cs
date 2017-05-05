using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ProductionOrderToProductionOrderWorkSchedule
    {
        public int ProductionOrderToProductionOrderWorkScheduleId { get; set; }
        public int? ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; }
        public int? ProductionOrderWorkScheduleId { get; set; }
        public ProductionOrderWorkSchedule ProductionOrderWorkSchedule { get; set; }
    }
}
