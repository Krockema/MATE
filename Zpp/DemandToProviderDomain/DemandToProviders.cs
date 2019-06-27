using System.Collections;
using System.Collections.Generic;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp.DemandToProviderDomain
{
    /**
     * Maps one demand to max. two providers (at least one of the providers must be exhausted,
     * the demand must be satisfied by both providers)
     */
    public class DemandToProviders : IDemandToProviders
    {
        private readonly Dictionary<Demand, Providers> _demandToProviders = new Dictionary<Demand, Providers>();
        private readonly Dictionary<Provider, Demands> _providerToDemands = new Dictionary<Provider, Demands>();

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

        public void SatisfyDemandWithProviders(Demand demand, Providers providers)
        {
            const int maxProvidersPerDemand = 2;
            if (providers.Size() > maxProvidersPerDemand)
            {
                throw new MrpRunException($"One demand must not need more than {maxProvidersPerDemand} providers.");
            }
            _demandToProviders.Add(demand, providers);
            foreach (var provider in providers.GetAll())
            {
                _providerToDemands.Add(provider, );
            }
        }

        public Provider FindNonExhaustedProvider(Demand demand)
        {
            throw new System.NotImplementedException();
        }
    }
}