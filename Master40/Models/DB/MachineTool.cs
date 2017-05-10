using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class MachineTool
    {
        public int MachineToolId { get; set; }
        public int MachineId { get; set; }
        public string Name { get; set; }
        public Machine Machine { get; set; }
        public int SetupTime { get; set; }
    }
}
