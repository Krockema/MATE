using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataModel
{
    public class M_ResourceCapabilityProvider : BaseEntity
    {
        public string Name { get; set; }
        public int ResourceCapabilityId { get; set; }
        public M_ResourceCapability ResourceCapability { get; set; }
        public ICollection<M_ResourceSetup> ResourceSetups { get; set; }

    }
}
