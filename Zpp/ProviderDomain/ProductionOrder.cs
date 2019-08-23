using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.DemandDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.LotSize;
using Zpp.SchedulingDomain;
using Zpp.Utils;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_ProductionOrder
     */
    public class ProductionOrder : Provider
    {
        public ProductionOrder(IProvider provider, IDbMasterDataCache dbMasterDataCache) : base(
            provider, dbMasterDataCache)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="article"></param>
        /// <param name="dbTransactionData"></param>
        /// <param name="dbMasterDataCache"></param>
        /// <param name="parentProductionOrder"></param>
        /// <param name="quantity">of production article to produce
        /// --> is used for childs as: articleBom.Quantity * quantity</param>
        /// <returns></returns>
        private static Demands CreateProductionOrderBoms(M_Article article,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            Provider parentProductionOrder, Quantity quantity)
        {
            M_Article readArticle = dbTransactionData.M_ArticleGetById(article.GetId());
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                List<Demand> newDemands = new List<Demand>();
                Dictionary<M_Operation, ProductionOrderOperation>
                    alreadyCreatedProductionOrderOperations =
                        new Dictionary<M_Operation, ProductionOrderOperation>();

                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    ProductionOrderOperation productionOrderOperation = null;
                    if (articleBom.OperationId == null)
                    {
                        throw new MrpRunException(
                            "Every PrOBom must have an operation. Add an operation to the articleBom.");
                    }

                    // load articleBom.Operation
                    if (articleBom.Operation == null)
                    {
                        articleBom.Operation =
                            dbMasterDataCache.M_OperationGetById(
                                new Id(articleBom.OperationId.GetValueOrDefault()));
                    }

                    // don't create an productionOrderOperation twice
                    if (alreadyCreatedProductionOrderOperations.ContainsKey(articleBom.Operation))
                    {
                        productionOrderOperation =
                            alreadyCreatedProductionOrderOperations[articleBom.Operation];
                    }

                    ProductionOrderBom newProductionOrderBom =
                        ProductionOrderBom.CreateProductionOrderBom(articleBom,
                            parentProductionOrder, dbMasterDataCache, quantity,
                            productionOrderOperation);

                    if (alreadyCreatedProductionOrderOperations.ContainsKey(articleBom.Operation) ==
                        false)
                    {
                        alreadyCreatedProductionOrderOperations.Add(articleBom.Operation,
                            newProductionOrderBom.GetProductionOrderOperation(dbTransactionData));
                    }

                    if (newProductionOrderBom.HasOperation() == false)
                    {
                        throw new MrpRunException(
                            "Every PrOBom must have an operation. Add an operation to the articleBom.");
                    }


                    newDemands.Add(newProductionOrderBom);
                }

                // backwards scheduling
                OperationBackwardsSchedule lastOperationBackwardsSchedule =
                    new OperationBackwardsSchedule(
                        parentProductionOrder.GetDueTime(dbTransactionData), null, null);

                IEnumerable<ProductionOrderOperation> sortedProductionOrderOperations = newDemands
                    .Select(x =>
                        ((ProductionOrderBom) x).GetProductionOrderOperation(dbTransactionData))
                    .OrderByDescending(x => x.GetValue().HierarchyNumber);

                foreach (var productionOrderOperation in sortedProductionOrderOperations)
                {
                    lastOperationBackwardsSchedule =
                         productionOrderOperation.ScheduleBackwards(
                            lastOperationBackwardsSchedule, parentProductionOrder.GetDueTime(dbTransactionData));
                }


                return new ProductionOrderBoms(newDemands);
            }

            return null;
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
            IDbTransactionData dbTransactionData, Provider parentProvider, Quantity quantity)
        {
            _dependingDemands = CreateProductionOrderBoms(article, dbTransactionData,
                _dbMasterDataCache, parentProvider, quantity);
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"P(PrO);{base.GetGraphizString(dbTransactionData)}";
            return graphizString;
        }

        public override DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            T_ProductionOrder productionOrder = (T_ProductionOrder) _provider;
            return new DueTime(productionOrder.DueTime);
        }

        public override DueTime GetStartTime(IDbTransactionData dbTransactionData)
        {
            return null;
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
    }
}