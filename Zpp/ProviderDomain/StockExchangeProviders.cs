using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.ProviderDomain
{
    /**
     * wraps the collection with all stockExchangeProviders
     */
    public class StockExchangeProviders : Providers
    {
        public StockExchangeProviders(List<T_StockExchange> iDemands, IDbMasterDataCache dbMasterDataCache) : base(ToProviders(iDemands, dbMasterDataCache))
        {
        }

        private static List<Provider> ToProviders(List<T_StockExchange> iProviders, IDbMasterDataCache dbMasterDataCache)
        {
            List<Provider> providers = new List<Provider>();
            foreach (var iProvider in iProviders)
            {
                providers.Add(new StockExchangeProvider(iProvider, null, dbMasterDataCache));
            }

            return providers;
        }
    }
}