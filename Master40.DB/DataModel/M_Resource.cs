using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Resource : BaseEntity
    {
        public string Name { get; set; }
        public int Count { get; set; }
        /*
         * Defines a list of Capabilities that can be 
         */
        public virtual ICollection<M_ResourceSetup> UsedInResourceSetups { get; set; }
        public virtual ICollection<M_ResourceSetup> RequiresResourceSetups { get; set; }

        public int Capacity { get; set; }

        [NotMapped]
        public object IResourceRef { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}