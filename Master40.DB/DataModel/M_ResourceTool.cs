using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_ResourceTool : BaseEntity
    {
        public string Name { get; set; }

        public string Discription { get; set; }
        [JsonIgnore]
        public virtual ICollection<M_ResourceSetup> ResourceSetups { get; set; }
    }
}
