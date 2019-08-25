using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp
{
    public interface IProductionOrderCreator
    {
        ProductionOrders CreateProductionOrder(IDbMasterDataCache dbMasterDataCache,
            IDbTransactionData dbTransactionData, Demand demand, Quantity quantity);
    }
}