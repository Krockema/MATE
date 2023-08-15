﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.ResourceAgent.Types
{
    public class CapabilityProviderManager
    {
        private IImmutableSet<M_ResourceCapabilityProvider> _resourceCapabilityProviders { get; set; }
        private CapabilityProviderInUse _capabilityInUse { get; set; }
        public CapabilityProviderManager(List<M_ResourceCapabilityProvider> resourceCapabilityProvider)
        {
            _resourceCapabilityProviders = resourceCapabilityProvider.ToImmutableHashSet();
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

        internal TimeSpan GetSetupDurationBy(int resourceCapabilityId)
        {
            //TODO Take care if one Capability can be done by multiply tools
            var setupTime = _resourceCapabilityProviders.First(x => x.ResourceCapabilityId == resourceCapabilityId)
                                                        .ResourceSetups.Sum(x => x.SetupTime);
            return setupTime;
        }

        internal IImmutableSet<M_ResourceCapabilityProvider> GetAllCapabilityProvider()
        {
            return _resourceCapabilityProviders;
        }

        internal List<M_ResourceCapability> GetAllCapabilities()
        {
            return _resourceCapabilityProviders.Select(x => x.ResourceCapability).ToList();
        }

    }
}
