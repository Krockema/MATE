using System.Collections.Generic;
using Master40.DB.DB.Models;

namespace Master40.BusinessLogicCentral.HelperCapacityPlanning
{
    public class MachineGroupProductionOrderWorkSchedule
    {
        public int Id { get; set; }
        public int MachineGroupId { get; set; }
        public MachineGroup MachineGroup { get; set; }
        public virtual List<ProductionOrderWorkSchedulesByTimeStep> ProductionOrderWorkSchedulesByTimeSteps { get; set; }
    }
}