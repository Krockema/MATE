using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class MachineGroup
    {
        [Key]
        public int MachineGroupId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; }
        public virtual ICollection<ProductionOrderWorkSchedule> ProductionOrderWorkSchedules { get; set; }
        public virtual ICollection<Machine> Machines  { get; set; }
    }
}
