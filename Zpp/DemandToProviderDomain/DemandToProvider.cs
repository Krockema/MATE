using System.Collections;
using System.Collections.Generic;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp.DemandToProviderDomain
{
    public class DemandToProvider : IDemandToProviderLogic
    {
        private readonly Dictionary<Demand, Providers> _demandToProviders = new Dictionary<Demand, Providers>();
        private readonly Dictionary<Provider, Demand> _providerToDemand = new Dictionary<Provider, Demand>();

        public DemandToProvider()
        {
            
        }

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
    }
}