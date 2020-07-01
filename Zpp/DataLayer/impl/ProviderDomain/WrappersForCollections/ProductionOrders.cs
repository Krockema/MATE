using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;

namespace Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections
{
    /**
     * wraps the collection with all productionOrders
     */
    public sealed class ProductionOrders : Providers
    {
        public ProductionOrders(List<Provider> providers)
        {
            AddAll(providers);
        }

        public ProductionOrders()
        {
        }

        public ProductionOrders(Provider provider)
        {
            Add(provider);
        }
        
        public ProductionOrders(List<T_ProductionOrder> iDemands)
        {
            AddAll(ToProviders(iDemands));
        }

        private static List<Provider> ToProviders(List<T_ProductionOrder> Providers)
        {
            List<Provider> providers = new List<Provider>();
            foreach (var iProvider in Providers)
            {
                providers.Add(new ProductionOrder(iProvider));
            }

            return providers;
        }

    }
}