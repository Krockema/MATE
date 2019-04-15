using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Machine : BaseEntity
    {
        public static string MACHINEGROUP_FKEY = "MachineGroup";

        public string Name { get; set; }
        public int Count { get; set; }
        public int MachineGroupId { get; set; }
        [JsonIgnore]
        public M_MachineGroup MachineGroup { get; set; }
        public int Capacity { get; set; }
        [JsonIgnore]
        public virtual ICollection<M_MachineTool> MachineTools { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderOperation> ProductionOrderWorkSchedules { get; set; }
    }
}
