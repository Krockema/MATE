using Akka.Actor;
using Master40.DB.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class CapabilityDefinition
    {
        public M_ResourceCapability ResourceCapability { get; private set; } = new M_ResourceCapability();
        
        public CapabilityDefinition(M_ResourceCapability resourceCapability)
        {
            this.ResourceCapability = resourceCapability;
        }

        public bool HasCapability(M_ResourceCapability resourceCapability)
        {
            return ResourceCapability.Id == resourceCapability.Id;
        }

        public void AddResourceRef(int resourceId, IActorRef resourceRef)
        {
            foreach (var provider in this.ResourceCapability.ResourceCapabilityProvider)
            {
                foreach (var setup in provider.ResourceSetups)
                {
                    if (setup.ResourceId == resourceId )
                    {
                        setup.Resource.IResourceRef = resourceRef;
                    }
                }
            }
        }

        internal List<M_ResourceCapabilityProvider> GetAllCapabilityProvider()
        {
            return ResourceCapability.ResourceCapabilityProvider.ToList();
        }
    }
}
