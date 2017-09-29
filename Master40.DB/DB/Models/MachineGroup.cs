using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class MachineGroup : BaseEntity
    {
        public string Name { get; set; }
        [JsonIgnore]
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; }
        public string ImageUrl { get; set; }
        public int Stage { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductionOrderWorkSchedule> ProductionOrderWorkSchedules { get; set; }
        [JsonIgnore]
        public virtual ICollection<Machine> Machines  { get; set; }
    }
}
