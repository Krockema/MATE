using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.Wrappers
{
    /**
     * wraps the collection with all stockExchangeProviders
     */
    public class StockExchangeProviders : Providers
    {
        public StockExchangeProviders(List<T_StockExchange> iDemands) : base(ToProviders(iDemands))
        {
        }

        private static List<Provider> ToProviders(List<T_StockExchange> iProviders)
        {
            List<Provider> providers = new List<Provider>();
            foreach (var iProvider in iProviders)
            {
                providers.Add(new StockExchangeProvider(iProvider, null));
            }

            return providers;
        }
    }
}