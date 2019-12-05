using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Resource : BaseEntity
    {
        // TODO: why is this required ?
        // public int ResourceId { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        /*
         * Defines a list of Skills that can be 
         */
        public virtual ICollection<M_ResourceSetup> ResourceSetups { get; set; }
        public virtual ICollection<M_ResourceSkill> ResourceSkills { get; set; }

        public int Capacity { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrderOperation> ProductionOrderOperations { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
