using System.Collections.Generic;

namespace Master40.DB.Models
{
    public class Machine : BaseEntity
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int MachineGroupId { get; set; }
        public MachineGroup MachineGroup { get; set; }
        public int Capacity { get; set; }
        public virtual ICollection<MachineTool> MachineTools { get; set; }
    }
}
