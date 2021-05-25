using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;

namespace Mate.Production.Core.Agents.HubAgent.Types
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
                    if (setup.ResourceId == resourceId)
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

        internal bool AddCapabilityProvider(M_ResourceCapabilityProvider capabilityProvider)
        {
            var currentCapabilityProvider = ResourceCapability.ResourceCapabilityProvider.SingleOrDefault(x => x.Id == capabilityProvider.Id);
            if (currentCapabilityProvider == null)
            {
                ResourceCapability.ResourceCapabilityProvider.Add(capabilityProvider);
                return true;
            }

            return false;

        }
    }
}