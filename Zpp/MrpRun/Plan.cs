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
        private IDemandToProviderTable _demandToProviderTable;

        public Plan(IDemands demands, IProviders providers, IDemandToProviderTable demandToProviders)
        {
            _demands = demands;
            _providers = providers;
            _demandToProviderTable = demandToProviders;
        }

        public IDemands GetDemands()
        {
            return _demands;
        }

        public IProviders GetProviders()
        {
            return _providers;
        }

        public IDemandToProviderTable GetDemandToProviders()
        {
            return _demandToProviderTable;
        }
    }
}