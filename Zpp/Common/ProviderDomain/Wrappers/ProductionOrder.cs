using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.DbCache;
using Zpp.Mrp.ProductionManagement;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.ProviderDomain.Wrappers
{
    /**
     * wraps T_ProductionOrder
     */
    public class ProductionOrder : Provider
    {
        private readonly T_ProductionOrder _productionOrder;
        public ProductionOrder(IProvider provider, IDbMasterDataCache dbMasterDataCache) : base(
            provider, dbMasterDataCache)
        {
            _productionOrder = (T_ProductionOrder)provider;
        }

        public override IProvider ToIProvider()
        {
            return (T_ProductionOrder) _provider;
        }

        public override Id GetArticleId()
        {
            Id articleId = new Id(((T_ProductionOrder) _provider).ArticleId);
            return articleId;
        }

        public override void CreateDependingDemands(M_Article article,
            IDbTransactionData dbTransactionData, Provider parentProvider, Quantity demandedQuantity)
        {
            _dependingDemands = ProductionManager.CreateProductionOrderBoms(article, dbTransactionData,
                _dbMasterDataCache, parentProvider, demandedQuantity);
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"P(PrO);{base.GetGraphizString(dbTransactionData)}";
            return graphizString;
        }

        public override DueTime GetDueTime(IDbTransactionData dbTransactionData = null)
        {
            T_ProductionOrder productionOrder = (T_ProductionOrder) _provider;
            return new DueTime(productionOrder.DueTime);
        }

        public override DueTime GetStartTime(IDbTransactionData dbTransactionData)
        {
            return GetDueTime(dbTransactionData);
        }

        public ProductionOrderBoms GetProductionOrderBoms(IDbTransactionData dbTransactionData)
        {
            return dbTransactionData.GetAggregator().GetProductionOrderBomsOfProductionOrder(this);
        }

        public bool HasOperations(IDbTransactionData dbTransactionData)
        {
            List<ProductionOrderOperation> productionOrderOperations = dbTransactionData
                .GetAggregator().GetProductionOrderOperationsOfProductionOrder(this);
            if (productionOrderOperations == null)
            {
                return false;
            }

            return productionOrderOperations.Any();
        }

        public override void SetDueTime(DueTime newDueTime, IDbTransactionData dbTransactionData)
        {
            T_ProductionOrder productionOrder = (T_ProductionOrder) _provider;
            productionOrder.DueTime = newDueTime.GetValue();
        }

        public override void SetProvided(DueTime atTime)
        {
            throw new System.NotImplementedException();
        }
    }
}