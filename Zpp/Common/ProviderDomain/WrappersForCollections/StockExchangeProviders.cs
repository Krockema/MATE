using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;

namespace Zpp.Common.ProviderDomain.WrappersForCollections
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
                if (iProvider.StockExchangeType.Equals(StockExchangeType.Demand))
                {
                    continue;
                }
                providers.Add(new StockExchangeProvider(iProvider, dbMasterDataCache));
            }

            return providers;
        }
    }
}