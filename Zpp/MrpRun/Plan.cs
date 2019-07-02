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
        private IDbTransactionData _dbTransactionData;

        public Plan(IDemands demands, IProviders providers,
            IDemandToProviderTable demandToProviders, IDbTransactionData dbTransactionData)
        {
            _demands = demands;
            _providers = providers;
            _demandToProviderTable = demandToProviders;
            _dbTransactionData = dbTransactionData;
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

        public IDbTransactionData GetDbTransactionData()
        {
            return _dbTransactionData;
        }
    }
}