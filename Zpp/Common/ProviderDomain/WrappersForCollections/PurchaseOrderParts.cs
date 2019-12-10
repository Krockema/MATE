using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;

namespace Zpp.Common.ProviderDomain.WrappersForCollections
{
    /**
     * wraps the collection with all purchaseOrderParts
     */
    public class PurchaseOrderParts : Providers
    {
        public PurchaseOrderParts(List<Provider> providers) : base(providers)
        {
        }
        
        public PurchaseOrderParts(List<T_PurchaseOrderPart> iDemands, IDbMasterDataCache dbMasterDataCache) : base(ToProviders(iDemands, dbMasterDataCache))
        {
        }

        private static List<Provider> ToProviders(List<T_PurchaseOrderPart> iProviders, IDbMasterDataCache dbMasterDataCache)
        {
            List<Provider> providers = new List<Provider>();
            foreach (var iProvider in iProviders)
            {
                providers.Add(new PurchaseOrderPart(iProvider, null, dbMasterDataCache));
            }

            return providers;
        }
    }
}