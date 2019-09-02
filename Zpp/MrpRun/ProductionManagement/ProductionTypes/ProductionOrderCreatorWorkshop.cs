using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.Common.ProviderDomain.WrappersForCollections;
using Zpp.DbCache;
using Zpp.Utils;

namespace Zpp.MrpRun.ProductionManagement.ProductionTypes
{
    /**
     * Here one ProductionOrder with productionOrder.Quantity == given quantity will be created
     */
    public class ProductionOrderCreatorWorkshop : IProductionOrderCreator
    {
        public ProductionOrderCreatorWorkshop()
        {
            if (Configuration.Configuration.ProductionType.Equals(ProductionType.WorkshopProduction) == false)
            {
                throw new MrpRunException("This is class is intended for productionType WorkshopProduction.");
            }
        }

        public ProductionOrders CreateProductionOrder(IDbMasterDataCache dbMasterDataCache,
            IDbTransactionData dbTransactionData, Demand demand, Quantity quantity)
        {
            T_ProductionOrder tProductionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            tProductionOrder.DueTime = demand.GetDueTime(dbTransactionData).GetValue();
            tProductionOrder.Article = demand.GetArticle();
            tProductionOrder.ArticleId = demand.GetArticle().Id;
            tProductionOrder.Name = $"ProductionOrder for Demand {demand.GetArticle()}";
            tProductionOrder.Quantity = quantity.GetValue();

            ProductionOrder productionOrder =
                new ProductionOrder(tProductionOrder, dbMasterDataCache);

            productionOrder.CreateDependingDemands(demand.GetArticle(), dbTransactionData,
                productionOrder, productionOrder.GetQuantity());

            return new ProductionOrders(productionOrder);
        }
    }
}