using System.Collections.Generic;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp
{
    public class Plan
    {
        private IDemands _demands;
        private IProviders _providers;

        public Plan(IDemands demands, IProviders providers)
        {
            _demands = demands;
            _providers = providers;
        }

        public IDemands GetDemands()
        {
            return _demands;
        }

        public IProviders GetProviders()
        {
            return _providers;
        }
    }
}