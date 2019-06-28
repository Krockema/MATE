using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.ProviderDomain
{
    /**
     * wraps the collection with all productionOrders
     */
    public class ProductionOrders : Providers
    {
        public ProductionOrders(List<Provider> providers) : base(providers)
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
                providers.Add(new ProductionOrder(iProvider, null, dbMasterDataCache));
            }

            return providers;
        }

    }
}