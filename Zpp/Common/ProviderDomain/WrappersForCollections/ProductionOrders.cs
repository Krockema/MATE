using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;

namespace Zpp.Common.ProviderDomain.WrappersForCollections
{
    /**
     * wraps the collection with all productionOrders
     */
    public class ProductionOrders : Providers
    {
        public ProductionOrders(List<Provider> providers) : base(providers)
        {
        }

        public ProductionOrders()
        {
        }

        public ProductionOrders(Provider provider) : base(provider)
        {
        }
        
        public ProductionOrders(List<T_ProductionOrder> iDemands, IDbMasterDataCache dbMasterDataCache) : base(ToProviders(iDemands, dbMasterDataCache))
        {
        }

        private static List<Provider> ToProviders(List<T_ProductionOrder> iProviders, IDbMasterDataCache dbMasterDataCache)
        {
            List<Provider> providers = new List<Provider>();
            foreach (var iProvider in iProviders)
            {
                providers.Add(new ProductionOrder(iProvider, dbMasterDataCache));
            }

            return providers;
        }

    }
}