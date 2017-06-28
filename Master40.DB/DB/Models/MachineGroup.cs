using System.Collections.Generic;

namespace Master40.DB.DB.Models
{
    public class MachineGroup : BaseEntity
    {
        public string Name { get; set; }
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; }
        public virtual ICollection<ProductionOrderWorkSchedule> ProductionOrderWorkSchedules { get; set; }
        public virtual ICollection<Machine> Machines  { get; set; }
    }
}
