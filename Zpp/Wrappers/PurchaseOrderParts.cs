using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.Wrappers
{
    /**
     * wraps the collection with all purchaseOrderParts
     */
    public class PurchaseOrderParts : Providers
    {
        public PurchaseOrderParts(List<Provider> providers) : base(providers)
        {
        }
        
        public PurchaseOrderParts(List<T_PurchaseOrderPart> iDemands) : base(ToProviders(iDemands))
        {
        }

        private static List<Provider> ToProviders(List<T_PurchaseOrderPart> iProviders)
        {
            List<Provider> providers = new List<Provider>();
            foreach (var iProvider in iProviders)
            {
                providers.Add(new PurchaseOrderPart(iProvider, null));
            }

            return providers;
        }
    }
}