using System;
using System.Collections;
using System.Collections.Generic;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp.DemandToProviderDomain
{
    
    public class DemandToProviders : IDemandToProviders
    {
        private readonly Dictionary<Demand, Providers> _demandToProviders = new Dictionary<Demand, Providers>();
        private readonly IProviderToDemands _providerToDemands = new ProviderToDemands();
        private const int MAX_PROVIDERS_PER_DEMAND = 2;

        public bool IsSatisfied(Demand demand)
        {
            if (_demandToProviders.ContainsKey(demand))
            {
                Providers providers = _demandToProviders[demand];
                if (providers.ProvideMoreThan(demand.GetQuantity()))
                {
                    return true;
                }
                
                return false;
            }

            return false;
        }

        public void AddProviderForDemand(Demand demand, Provider provider)
        {
            if (!_demandToProviders.ContainsKey(demand))
            {
                _demandToProviders.Add(demand, new Providers());
            }
            if (_demandToProviders[demand].Size() > MAX_PROVIDERS_PER_DEMAND)
            {
                throw new MrpRunException($"One demand must not have more than {MAX_PROVIDERS_PER_DEMAND} providers.");
            }
            _demandToProviders[demand].Add(provider);
            _providerToDemands.AddDemandForProvider(provider, demand);
            }

        public Provider FindNonExhaustedProvider(Demand demand)
        {
            return _providerToDemands.FindNonExhaustedProvider(demand);
        }

        public void AddProvidersForDemand(Demand demand, Providers providers)
        {
            foreach (var provider in providers.GetAll())
            {
                AddProviderForDemand(demand, provider);
            }
        }
    }
}