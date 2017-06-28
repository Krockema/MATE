using System.Collections.Generic;
using Master40.DB.DB.Models;

namespace Master40.BusinessLogicCentral.HelperCapacityPlanning
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