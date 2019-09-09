using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.Common.ProviderDomain.WrappersForCollections;
using Zpp.DbCache;
using Zpp.Utils;

namespace Zpp.Mrp.ProductionManagement.ProductionTypes
{
    /**
     * Here ProductionOrders.Count == given quantity productionOrders will be created
     */
    public class ProductionOrderCreatorAssemblyLine: IProductionOrderCreator
    {
        public ProductionOrderCreatorAssemblyLine()
        {
            if (Configuration.Configuration.ProductionType.Equals(ProductionType.AssemblyLine) == false)
            {
                throw new MrpRunException("This is class is intended for productionType AssemblyLine.");
            }
        }

        public ProductionOrders CreateProductionOrder(IDbMasterDataCache dbMasterDataCache,
            IDbTransactionData dbTransactionData, Demand demand, Quantity quantity)
        {
            ProductionOrders productionOrders = new ProductionOrders();
            
            for (int i = 0; i < quantity.GetValue(); i++)
            {

                T_ProductionOrder tProductionOrder = new T_ProductionOrder();
                // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
                tProductionOrder.DueTime = demand.GetDueTime(dbTransactionData).GetValue();
                tProductionOrder.Article = demand.GetArticle();
                tProductionOrder.ArticleId = demand.GetArticle().Id;
                tProductionOrder.Name = $"ProductionOrder for Demand {demand.GetArticle()}";
                tProductionOrder.Quantity = 1;

                ProductionOrder productionOrder =
                    new ProductionOrder(tProductionOrder, dbMasterDataCache);

                productionOrder.CreateDependingDemands(demand.GetArticle(), dbTransactionData,
                    productionOrder, productionOrder.GetQuantity());
                
                productionOrders.Add(productionOrder);
            }

            return productionOrders;
        }
    }
}