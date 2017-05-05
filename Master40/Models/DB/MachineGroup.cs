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
        public int Count { get; set; }
        public string Name { get; set; }
        public int WorkScheduleItemId { get; set; }
        public WorkSchedule WorkSchedule { get; set; }
        public virtual ICollection<Machine> Machines  { get; set; }
    }
}
