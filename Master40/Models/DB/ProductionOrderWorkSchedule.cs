using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ProductionOrderWorkSchedule
    {
        public int ProductionOrderWorkScheduleId { get; set; }
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public int? MachineToolId { get; set; }
        public MachineTool MachineTool { get; set; }
        public int? MachineGroupId { get; set; }
        public MachineGroup MachineGroup { get; set; }
        public ICollection<ProductionOrderToProductionOrderWorkSchedule> ProductionOrderToWorkSchedules { get; set; }
    }
}
