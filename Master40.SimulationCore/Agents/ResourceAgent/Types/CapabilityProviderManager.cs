using Master40.DB.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class CapabilityProviderManager
    {
        private List<M_ResourceCapabilityProvider> _resourceCapabilityProviders { get; set; } = new List<M_ResourceCapabilityProvider>();
        private CapabilityProviderInUse _capabilityInUse { get; set; }
        public CapabilityProviderManager(List<M_ResourceCapabilityProvider> resourceCapabilityProvider)
        {
            _resourceCapabilityProviders = resourceCapabilityProvider;
            _capabilityInUse = new CapabilityProviderInUse();

        }

        internal void Mount(int capabilityProviderId)
        {
            if(!AlreadyEquipped(capabilityProviderId))
            {
                var capabilityProvider = _resourceCapabilityProviders.Single(x => x.Id == capabilityProviderId);
                _capabilityInUse.Mount(capabilityProvider);
            }

        }

        public M_ResourceCapability GetCurrentUsedCapability()
        {
            return _capabilityInUse.ResourceCapabilityProvider?.ResourceCapability;
        }

        public int GetCurrentUsedCapabilityId()
        {
            if (_capabilityInUse.ResourceCapabilityProvider == null) return 0;
            return ((int)_capabilityInUse.ResourceCapabilityProvider?.ResourceCapability.Id);
        }

        /// <summary>
        /// Input: Capability
        /// </summary>
        /// <param name="resourceCapabilityId"></param>
        /// <returns></returns>
        internal bool AlreadyEquipped(int resourceCapabilityId)
        {
            if(_capabilityInUse.ResourceCapabilityProvider == null)
                return false;
            return _capabilityInUse.ResourceCapabilityProvider.ResourceCapabilityId == resourceCapabilityId;
        }

        internal M_ResourceCapabilityProvider GetCapabilityProviderByCapability(int resourceCapabilityId)
        {
            //TODO Take care if 1 Capability can be done by multiply tools
            var resourceCapabilityProvider = _resourceCapabilityProviders.First(x => x.ResourceCapability.Id == resourceCapabilityId);
            return resourceCapabilityProvider;
        }

        internal long GetSetupDurationBy(int resourceCapabilityId)
        {
            //TODO Take care if one Capability can be done by multiply tools
            var setupTime = _resourceCapabilityProviders.First(x => x.ResourceCapabilityId == resourceCapabilityId)
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

    }
}
