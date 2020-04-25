using Master40.DB.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class CapabilityProviderManager
    {
        private List<M_ResourceCapabilityProvider> _resourceCapabilityProviders { get; set; } = new List<M_ResourceCapabilityProvider>();
        private CapabilityInUse _capabilityInUse { get; set; } 
        public CapabilityProviderManager(List<M_ResourceCapabilityProvider> resourceCapabilityProvider)
        {
            _resourceCapabilityProviders = resourceCapabilityProvider;
            _capabilityInUse = new CapabilityInUse();
        }

        internal void Mount(int capabilityId)
        {
            if (!AlreadyEquipped(capabilityId))
            {
                var capabilityProvider = _resourceCapabilityProviders.Single(x => x.Id == capabilityId);
                _capabilityInUse.Mount(capabilityProvider);
            }

        }

        public M_ResourceCapability GetCurrentUsedCapability()
        {
            return _capabilityInUse.ResourceCapabilityProvider?.ResourceCapability;
        }

        internal bool AlreadyEquipped(int capabilityId)
        {
            if(_capabilityInUse.ResourceCapabilityProvider == null)
                return false;
            return _capabilityInUse.ResourceCapabilityProvider.ResourceCapabilityId == capabilityId;
        }

        internal M_ResourceCapabilityProvider GetCapabilityProviderByCapability(int capabilityProviderId)
        {
            //TODO Take care if 1 Capability can be done by multiply tools
            var resourceCapabilityProvider = _resourceCapabilityProviders.Single(x => x.Id == capabilityProviderId);
            return resourceCapabilityProvider;
        }

        internal long GetSetupDurationByCapabilityProvider(int resourceCapabilityProviderId)
        {
            //TODO Take care if 1 Capability can be done by multiply tools

            var setupTime = _resourceCapabilityProviders.Single(x => x.Id == resourceCapabilityProviderId)
                                                            .ResourceSetups.Sum(x => x.SetupTime);
            return setupTime;
        }

        internal List<M_ResourceCapabilityProvider> GetAllCapabilityProvider()
        {
            return _resourceCapabilityProviders;
        }
        internal List<M_ResourceCapability> GetAllCapabilities()
        {
            return _resourceCapabilityProviders.Select(x => x.ResourceCapability).ToList();
        }

        internal object GetCapabilityProviderName()
        {
            string capabilityProviderName = "was not set";
            if (_capabilityInUse.ResourceCapabilityProvider != null)
                capabilityProviderName = _capabilityInUse.ResourceCapabilityProvider.Name;
            return capabilityProviderName;
        }

        internal int CurrentSetupId => _capabilityInUse.SetupId();
        

    }
}
