using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;

namespace Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections
{
    /**
     * wraps the collection with all purchaseOrderParts
     */
    public sealed class PurchaseOrderParts : Providers
    {
        public PurchaseOrderParts(List<Provider> providers) 
        {
            AddAll(providers);
        }
        
        public PurchaseOrderParts(List<T_PurchaseOrderPart> iDemands)
        {
            AddAll(ToProviders(iDemands));
        }

        private static List<Provider> ToProviders(List<T_PurchaseOrderPart> Providers)
        {
            List<Provider> providers = new List<Provider>();
            foreach (var iProvider in Providers)
            {
                providers.Add(new PurchaseOrderPart(iProvider, null));
            }

            return providers;
        }
    }
}