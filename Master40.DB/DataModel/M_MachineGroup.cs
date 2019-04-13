using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_MachineGroup : BaseEntity
    {
        public string Name { get; set; }
        [JsonIgnore]
        public virtual ICollection<M_Operation> WorkSchedules { get; set; }
        public string ImageUrl { get; set; }
        public int Stage { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderOperation> ProductionOrderWorkSchedules { get; set; }
        [JsonIgnore]
        public virtual ICollection<M_Machine> Machines  { get; set; }

        public virtual ICollection<M_MachineTool> MachineTools { get; set; }

    }
}
