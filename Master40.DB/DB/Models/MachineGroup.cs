using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Master40.DB.Models
{
    public class MachineGroup : BaseEntity
    {
        public string Name { get; set; }
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; }
        public virtual ICollection<ProductionOrderWorkSchedule> ProductionOrderWorkSchedules { get; set; }
        public virtual ICollection<Machine> Machines  { get; set; }
    }
}
