using System.Collections.Generic;
using Zpp.DemandDomain;
using Zpp.DemandToProviderDomain;
using Zpp.ProviderDomain;

namespace Zpp
{
    public class Plan : IPlan
    {
        private IDemands _demands;
        private IProviders _providers;
        private IDemandToProviders _demandToProviders;

        public Plan(IDemands demands, IProviders providers, IDemandToProviders demandToProviders)
        {
            _demands = demands;
            _providers = providers;
            _demandToProviders = demandToProviders;
        }

        public IDemands GetDemands()
        {
            return _demands;
        }

        public IProviders GetProviders()
        {
            return _providers;
        }

        public IDemandToProviders GetDemandToProviders()
        {
            return _demandToProviders;
        }
    }
}