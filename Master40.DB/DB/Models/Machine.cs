using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class Machine : BaseEntity
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int MachineGroupId { get; set; }
        [JsonIgnore]
        public MachineGroup MachineGroup { get; set; }
        public int Capacity { get; set; }
        [JsonIgnore]
        public virtual ICollection<MachineTool> MachineTools { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductionOrderWorkSchedule> ProductionOrderWorkSchedules { get; set; }
    }
}
