using System.Collections.Generic;

namespace Master40.DB.Models
{
    public class ProductionOrderWorkSchedulesByTimeStep
    {
        public int Id { get; set; }
        public int Time { get; set; }
        public virtual List<ProductionOrderWorkSchedule> ProductionOrderWorkSchedules { get; set; }
    }
}