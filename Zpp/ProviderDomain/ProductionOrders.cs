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
        
        public ProductionOrders(List<T_ProductionOrder> iDemands) : base(ToProviders(iDemands))
        {
        }

        private static List<Provider> ToProviders(List<T_ProductionOrder> iProviders)
        {
            List<Provider> providers = new List<Provider>();
            foreach (var iProvider in iProviders)
            {
                providers.Add(new ProductionOrder(iProvider, null));
            }

            return providers;
        }

    }
}