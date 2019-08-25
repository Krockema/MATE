using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Zpp.ProductionDomain;
using Zpp.PurchaseDomain;

namespace Zpp.ProviderDomain
{
    /**
     * abstracts over PurchaseManager+ProductionManager
     */
    public class OrderManager : IProvidingManager
    {
        private readonly IProvidingManager _purchaseManager;
        private readonly IProvidingManager _productionManager;

        public OrderManager(IDbMasterDataCache dbMasterDataCache)
        {
            _purchaseManager = new PurchaseManager(dbMasterDataCache);
            _productionManager = new ProductionManager(dbMasterDataCache);
        }

        public Response Satisfy(Demand demand, Quantity demandedQuantity, IDbTransactionData dbTransactionData)
        {
            if (demand.GetArticle().ToBuild)
            {
                return _productionManager.Satisfy(demand,
                    demandedQuantity, dbTransactionData);
            }
            else
            {
                return _purchaseManager.Satisfy(demand,
                    demandedQuantity, dbTransactionData);
            }
        }
    }
}