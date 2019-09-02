using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.Common.DemandDomain;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.Common.ProviderDomain;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.Common.ProviderDomain.WrappersForCollections;
using Zpp.DbCache;
using Zpp.MrpRun.ProductionManagement.ProductionTypes;
using Zpp.MrpRun.Scheduling;
using Zpp.Utils;

namespace Zpp.MrpRun.ProductionManagement
{
    public class ProductionManager : IProvidingManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDbMasterDataCache _dbMasterDataCache;

        public ProductionManager(IDbMasterDataCache dbMasterDataCache)
        {
            _dbMasterDataCache = dbMasterDataCache;
        }

        public ResponseWithProviders Satisfy(Demand demand, Quantity demandedQuantity,
            IDbTransactionData dbTransactionData)
        {
            if (demand.GetArticle().ToBuild == false)
            {
                throw new MrpRunException("Must be a build article.");
            }

            IProviders productionOrders = CreateProductionOrder(demand, dbTransactionData,
                _dbMasterDataCache, demandedQuantity);

            Logger.Debug("ProductionOrder(s) created.");

            List<T_DemandToProvider> demandToProviders = new List<T_DemandToProvider>();
            
            foreach (var productionOrder in productionOrders)
            {
                T_DemandToProvider demandToProvider = new T_DemandToProvider()
                {
                    DemandId = demand.GetId().GetValue(),
                    ProviderId = productionOrder.GetId().GetValue(),
                    Quantity = productionOrder.GetQuantity().GetValue()
                };
                demandToProviders.Add(demandToProvider);
            }
            

            return new ResponseWithProviders(productionOrders, demandToProviders, demandedQuantity);
        }

        private ProductionOrders CreateProductionOrder(Demand demand,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            Quantity lotSize)
        {
            if (!demand.GetArticle().ToBuild)
            {
                throw new MrpRunException(
                    "You are trying to create a productionOrder for a purchaseArticle.");
            }
            
            IProductionOrderCreator productionOrderCreator;
            switch (Configuration.Configuration.ProductionType)
            {
                case ProductionType.AssemblyLine:
                    productionOrderCreator = new ProductionOrderCreatorAssemblyLine();
                    break;
                case ProductionType.WorkshopProduction:
                    productionOrderCreator = new ProductionOrderCreatorWorkshop();
                    break;
                case ProductionType.WorkshopProductionClassic:
                    productionOrderCreator = new ProductionOrderCreatorWorkshopClassic();
                    break;
                default:
                    productionOrderCreator = null;
                    break;
            }

            

            return productionOrderCreator.CreateProductionOrder(dbMasterDataCache, dbTransactionData, demand, lotSize);
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
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            Provider parentProductionOrder, Quantity quantity)
        {
            M_Article readArticle = dbTransactionData.M_ArticleGetById(article.GetId());
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                List<Demand> newDemands = new List<Demand>();
                IProductionOrderBomCreator productionOrderBomCreator;
                switch (Configuration.Configuration.ProductionType)
                {
                    case ProductionType.AssemblyLine:
                        productionOrderBomCreator = new ProductionOrderBomCreatorAssemblyLine();
                        break;
                    case ProductionType.WorkshopProduction:
                        productionOrderBomCreator = new ProductionOrderBomCreatorWorkshop();
                        break;
                    case ProductionType.WorkshopProductionClassic:
                        productionOrderBomCreator = new ProductionOrderBomCreatorWorkshopClassic();
                        break;
                    default:
                        productionOrderBomCreator = null;
                        break;
                }
                
                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    newDemands.AddRange(
                        productionOrderBomCreator.CreateProductionOrderBomsForArticleBom(
                            dbMasterDataCache, dbTransactionData, articleBom, quantity,
                            (ProductionOrder) parentProductionOrder));
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
                    lastOperationBackwardsSchedule = productionOrderOperation.ScheduleBackwards(
                        lastOperationBackwardsSchedule,
                        parentProductionOrder.GetDueTime(dbTransactionData));
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
            productionOrderOperation.Duration = articleBom.Operation.Duration * (int)quantity.GetValue();
            // Tool has no meaning yet, ignore it
            productionOrderOperation.MachineTool = articleBom.Operation.MachineTool;
            productionOrderOperation.MachineToolId = articleBom.Operation.MachineToolId;
            productionOrderOperation.MachineGroup = articleBom.Operation.MachineGroup;
            productionOrderOperation.MachineGroupId = articleBom.Operation.MachineGroupId;
            productionOrderOperation.ProducingState = ProducingState.Created;
            productionOrderOperation.ProductionOrder =
                (T_ProductionOrder) parentProductionOrder.ToIProvider();
            productionOrderOperation.ProductionOrderId =
                productionOrderOperation.ProductionOrder.Id;

            return productionOrderOperation;
        }
    }
}