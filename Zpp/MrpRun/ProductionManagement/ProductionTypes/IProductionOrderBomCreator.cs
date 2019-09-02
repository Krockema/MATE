using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;

namespace Zpp.MrpRun.ProductionManagement.ProductionTypes
{
    public interface IProductionOrderBomCreator
    {
        Demands CreateProductionOrderBomsForArticleBom(IDbMasterDataCache dbMasterDataCache,
            IDbTransactionData dbTransactionData, M_ArticleBom articleBom, Quantity quantity,
            ProductionOrder parentProductionOrder);
    }
}