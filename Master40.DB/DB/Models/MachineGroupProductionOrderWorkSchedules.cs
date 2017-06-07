using System.Collections.Generic;

namespace Master40.DB.Models
{
    public class MachineGroupProductionOrderWorkSchedule
    {
        public int Id { get; set; }
        public int MachineGroupId { get; set; }
        public virtual List<ProductionOrderWorkSchedulesByTimeStep> ProductionOrderWorkSchedulesByTimeSteps { get; set; }
    }
}