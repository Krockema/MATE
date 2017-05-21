using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class Machine
    {
        public int MachineId { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int MachineGroupId { get; set; }
        public MachineGroup MachineGroup { get; set; }
        public int Capacity { get; set; }
        public virtual ICollection<MachineTool> MachineTools { get; set; }
    }
}
