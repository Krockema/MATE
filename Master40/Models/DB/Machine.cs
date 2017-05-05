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
        public string Capacity { get; set; }
        public virtual ICollection<MachineTool> MachineTools { get; set; }
    }
}
