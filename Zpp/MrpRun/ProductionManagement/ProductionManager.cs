using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp.ProductionDomain
{
    public class ProductionManager : IProvidingManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDbMasterDataCache _dbMasterDataCache;

        public ProductionManager(IDbMasterDataCache dbMasterDataCache)
        {
            _dbMasterDataCache = dbMasterDataCache;
        }

        public Response Satisfy(Demand demand, Quantity demandedQuantity,
            IDbTransactionData dbTransactionData)
        {
            if (demand.GetArticle().ToBuild == false)
            {
                throw new MrpRunException("Must be a build article.");
            }

            ProductionOrder productionOrder = CreateProductionOrder(demand, dbTransactionData,
                _dbMasterDataCache, demandedQuantity);

            Logger.Debug("ProductionOrder created.");


            T_DemandToProvider demandToProvider = new T_DemandToProvider()
            {
                DemandId = demand.GetId().GetValue(),
                ProviderId = productionOrder.GetId().GetValue(),
                Quantity = demandedQuantity.GetValue()
            };

            return new Response(productionOrder, demandToProvider, demandedQuantity);
        }

        private ProductionOrder CreateProductionOrder(Demand demand,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            Quantity lotSize)
        {
            if (!demand.GetArticle().ToBuild)
            {
                throw new MrpRunException(
                    "You are trying to create a productionOrder for a purchaseArticle.");
            }

            T_ProductionOrder tProductionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            tProductionOrder.DueTime = demand.GetDueTime(dbTransactionData).GetValue();
            tProductionOrder.Article = demand.GetArticle();
            tProductionOrder.ArticleId = demand.GetArticle().Id;
            tProductionOrder.Name = $"ProductionOrder for Demand {demand.GetArticle()}";
            tProductionOrder.Quantity = lotSize.GetValue();

            ProductionOrder productionOrder =
                new ProductionOrder(tProductionOrder, dbMasterDataCache);

            productionOrder.CreateDependingDemands(demand.GetArticle(), dbTransactionData,
                productionOrder, productionOrder.GetQuantity());

            return productionOrder;
        }
    }
}