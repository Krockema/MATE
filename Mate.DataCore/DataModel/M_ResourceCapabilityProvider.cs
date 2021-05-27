using System.Collections.Generic;

namespace Mate.DataCore.DataModel
{
    public class M_ResourceCapabilityProvider : BaseEntity
    {
        public string Name { get; set; }
        public int ResourceCapabilityId { get; set; }
        public M_ResourceCapability ResourceCapability { get; set; }
        public ICollection<M_ResourceSetup> ResourceSetups { get; set; }

    }
}
