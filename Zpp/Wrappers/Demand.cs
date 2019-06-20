using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.WrappersForPrimitives;

namespace Zpp.Wrappers
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Demand : WIDemand
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected IDemand _demand;

        public Demand(IDemand demand)
        {
            _demand = demand;
        }
        
        public Demand()
        {
        }
        
        public WIProvider CreateProvider(IDbCache dbCache)
        {
            if (_demand.GetArticle().ToBuild)
            {
                return CreateProductionOrder(dbCache);
            }
            else if (_demand.GetArticle().ToPurchase)
            {
                return createPurchaseOrderPart();
            }
        }
        
        private WIProvider CreateProductionOrder(IDbCache dbCache)
        {
            WIProvider provider;
            WT_ProductionOrder productionOrder = new WT_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            productionOrder.DueTime = _demand.GetDueTime();
            productionOrder.Article = _demand.GetArticle();
            productionOrder.ArticleId = _demand.GetArticle().Id;
            productionOrder.Name = $"ProductionOrder{_demand.Id}";
            // connects this provider with table T_Provider
            productionOrder.Provider = new T_Provider();
            productionOrder.Quantity = _demand.GetQuantity();
            

            // TODO: check following navigation properties are created
            /*List<T_ProductionOrderBom> productionOrderBoms = new List<T_ProductionOrderBom>();
            List<T_ProductionOrderOperation> productionOrderWorkSchedule = new List<T_ProductionOrderOperation>();
            List<T_ProductionOrderBom> prodProductionOrderBomChilds = new List<T_ProductionOrderBom>();*/

            List<WIDemand> productionOrderBoms = ProcessArticleBoms(productionOrder, dbCache);
            provider = new WT_ProductionOrder(productionOrder, productionOrderBoms);
            Logger.Debug("ProductionOrder created.");
            return provider;
        }

        private List<WIDemand> ProcessArticleBoms( 
            WT_ProductionOrder productionOrder, IDbCache dbCache)
        {
            List<WIDemand> productionOrderBoms = new List<WIDemand>();
            
            M_Article readArticle = dbCache.M_ArticleGetById(_demand.GetArticle().Id);
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    WT_ProductionOrderBom productionOrderBom = new WT_ProductionOrderBom(articleBom,
                        productionOrder);
                    productionOrderBoms.Add(productionOrderBom);
                    
                }
            }

            return productionOrderBoms;
        }

        

        private T_ProductionOrderOperation CreateProductionOrderBomOperation(
            M_ArticleBom articleBom)
        {
            if (articleBom.ArticleChild.ToPurchase)
            {
                return null;
            }
            T_ProductionOrderOperation productionOrderOperation = new T_ProductionOrderOperation();
            // TODO: add not only entities but also the ids !!! --> only ids should be enough???
            productionOrderOperation.Name = articleBom.Operation.Name;
            productionOrderOperation.HierarchyNumber = articleBom.Operation.HierarchyNumber;
            productionOrderOperation.Duration = articleBom.Operation.Duration;
            productionOrderOperation.MachineTool = articleBom.Operation.MachineTool;
            productionOrderOperation.MachineToolId = articleBom.Operation.MachineToolId;
            productionOrderOperation.MachineGroup = articleBom.Operation.MachineGroup;
            productionOrderOperation.MachineGroupId = articleBom.Operation.MachineGroupId;
            productionOrderOperation.ProducingState = ProducingState.Created;
            
            // TODO: external Algo needed
            
            // for machine utilisation
            // productionOrderOperation.Machine,
            
            // for simulation
            // Start, End,
            
            // for backward scheduling
            // StartBackward, EndBackward,
            
            // for forward scheduling
            // StartForward, EndForward,

            return productionOrderOperation;
        }

        // TODO: use this
        private int CalculatePriority(int dueTime, int operationDuration, int currentTime)
        {
            return dueTime - operationDuration - currentTime;
        }
        
        public WIProvider createPurchaseOrderPart()
        {
            // currently only one businessPartner per article
            M_ArticleToBusinessPartner articleToBusinessPartner = _demand.GetArticle()
                .ArticleToBusinessPartners.OfType<M_ArticleToBusinessPartner>().First();
            T_PurchaseOrder purchaseOrder = new T_PurchaseOrder();
            // [Name],[DueTime],[BusinessPartnerId]
            purchaseOrder.DueTime = _demand.GetDueTime();
            purchaseOrder.BusinessPartner = articleToBusinessPartner.BusinessPartner;
            purchaseOrder.Name = $"PurchaseOrder{_demand.GetArticle().Name} for " +
                                 $"businessPartner {purchaseOrder.BusinessPartner.Id}";


            // _demand cannot be fulfilled in time
            if (articleToBusinessPartner.DueTime > _demand.GetDueTime())
            {
                Logger.Error($"Article {_demand.GetArticle().Id} from _demand {_demand.Id} " +
                             $"should be available at {_demand.GetDueTime()}, but " +
                             $"businessPartner {articleToBusinessPartner.BusinessPartner.Id} " +
                             $"can only deliver at {articleToBusinessPartner.DueTime}.");
            }

            // init a new purchaseOderPart
            T_PurchaseOrderPart purchaseOrderPart = new T_PurchaseOrderPart();

            // [PurchaseOrderId],[ArticleId],[Quantity],[State],[ProviderId]
            purchaseOrderPart.PurchaseOrder = purchaseOrder;
            purchaseOrderPart.Article = _demand.GetArticle();
            purchaseOrderPart.Quantity =
                PurchaseManagerUtils.calculateQuantity(articleToBusinessPartner,
                    _demand.GetQuantity());
            purchaseOrderPart.State = State.Created;
            // connects this provider with table T_Provider
            purchaseOrderPart.Provider = new T_Provider();


            Logger.Debug("PurchaseOrderPart created.");
            return new WT_PurchaseOrderPart(purchaseOrderPart, null);
        }
    }
}