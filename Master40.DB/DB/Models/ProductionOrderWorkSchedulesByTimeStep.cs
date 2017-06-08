using System.Collections.Generic;

namespace Master40.DB.Models
{
    public class ProductionOrderWorkSchedulesByTimeStep
    {
        public int Id { get; set; }
        public int Time { get; set; }
        public int MachineGroupProductionOrderWorkScheduleId { get; set; }
        public MachineGroupProductionOrderWorkSchedule MachineGroupProductionOrderWorkSchedule { get; set; }
        public virtual List<ProductionOrderWorkSchedule> ProductionOrderWorkSchedules { get; set; }
    }
}