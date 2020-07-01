using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.Mrp2.impl.Mrp1.impl.Production.impl.ProductionTypes;
using Zpp.Scheduling.impl;
using Zpp.Util;

namespace Zpp.Mrp2.impl.Mrp1.impl.Production.impl
{
    public class ProductionManager
    {
        

        private readonly IDbMasterDataCache _dbMasterDataCache =
            ZppConfiguration.CacheManager.GetMasterDataCache();

        private readonly ICacheManager _cacheManager = ZppConfiguration.CacheManager;

        public ProductionManager()
        {
        }

        /**
         * SE:I --> satisfy by orders PrOBom
         */
        public EntityCollector Satisfy(Demand demand, Quantity demandedQuantity)
        {
            if (demand.GetArticle().ToBuild == false)
            {
                throw new MrpRunException("Must be a build article.");
            }

            EntityCollector entityCollector = CreateProductionOrder(demand, demandedQuantity);

            foreach (var provider in entityCollector.GetProviders())
            {
                T_DemandToProvider demandToProvider = new T_DemandToProvider()
                {
                    DemandId = demand.GetId().GetValue(),
                    ProviderId = provider.GetId().GetValue(),
                    Quantity = provider.GetQuantity().GetValue()
                };
                entityCollector.Add(demandToProvider);
            }


            return entityCollector;
        }

        private EntityCollector CreateProductionOrder(Demand demand, Quantity lotSize)
        {
            if (!demand.GetArticle().ToBuild)
            {
                throw new MrpRunException(
                    "You are trying to create a productionOrder for a purchaseArticle.");
            }

            IProductionOrderCreator productionOrderCreator = new ProductionOrderCreator();

            return productionOrderCreator.CreateProductionOrder(demand, lotSize);
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
        public static Demands CreateProductionOrderBoms(M_Article article,
            Provider parentProductionOrder, Quantity quantity)
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();

            M_Article readArticle = dbTransactionData.M_ArticleGetById(article.GetId());
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                List<Demand> newDemands = new List<Demand>();
                IProductionOrderBomCreator productionOrderBomCreator = new ProductionOrderBomCreator();

                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    newDemands.AddRange(
                        productionOrderBomCreator.CreateProductionOrderBomsForArticleBom(articleBom,
                            quantity, (ProductionOrder) parentProductionOrder));
                }
                
                // backwards scheduling
                OperationBackwardsSchedule lastOperationBackwardsSchedule = null;

                IEnumerable<ProductionOrderOperation> sortedProductionOrderOperations = newDemands
                    .Select(x => ((ProductionOrderBom) x).GetProductionOrderOperation())
                    .OrderByDescending(x => x.GetValue().HierarchyNumber);

                foreach (var productionOrderOperation in sortedProductionOrderOperations)
                {
                    lastOperationBackwardsSchedule = productionOrderOperation.ScheduleBackwards(
                        lastOperationBackwardsSchedule, parentProductionOrder.GetStartTimeBackward());
                }
                
                return new ProductionOrderBoms(newDemands);
            }

            return null;
        }

        public static T_ProductionOrderOperation CreateProductionOrderOperation(
            M_ArticleBom articleBom, Provider parentProductionOrder, Quantity quantity)
        {
            T_ProductionOrderOperation productionOrderOperation = new T_ProductionOrderOperation();
            productionOrderOperation = new T_ProductionOrderOperation();
            productionOrderOperation.Name = articleBom.Operation.Name;
            productionOrderOperation.HierarchyNumber = articleBom.Operation.HierarchyNumber;
            productionOrderOperation.Duration =
                articleBom.Operation.Duration * (int) quantity.GetValue();
            // Tool has no meaning yet, ignore it
            productionOrderOperation.ResourceToolId = null;
            productionOrderOperation.ResourceCapability = articleBom.Operation.ResourceCapability;
            productionOrderOperation.ResourceCapabilityId = articleBom.Operation.ResourceCapabilityId;
            productionOrderOperation.State = State.Created;
            productionOrderOperation.ProductionOrder =
                (T_ProductionOrder) parentProductionOrder.ToIProvider();
            productionOrderOperation.ProductionOrderId =
                productionOrderOperation.ProductionOrder.Id;

            return productionOrderOperation;
        }

        public EntityCollector CreateDependingDemands(Provider provider)
        {
            EntityCollector entityCollector = new EntityCollector();
            Demands dependingDemands = CreateProductionOrderBoms(provider.GetArticle(), provider,
                provider.GetQuantity());
            entityCollector.AddAll(dependingDemands);
            foreach (var dependingDemand in dependingDemands)
            {
                T_ProviderToDemand providerToDemand = new T_ProviderToDemand(provider.GetId(),
                    dependingDemand.GetId(), dependingDemand.GetQuantity());
                entityCollector.Add(providerToDemand);
            }

            return entityCollector;
        }
    }
}