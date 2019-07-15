using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Resource : BaseEntity
    {
        public int ResourceId { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        /*
         * Defines a list of Skills that can be 
         */
        public int MachineGroupId { get; set; }
        public M_MachineGroup MachineGroup { get; set; }
        [JsonIgnore]
        public virtual ICollection<M_ResourceToResourceTool> ResourceToResourceTools { get; set; }
        public int Capacity { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderOperation> ProductionOrderWorkSchedules { get; set; }
    }
}
