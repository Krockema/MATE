using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;

namespace Zpp
{
    public class ProductionManager
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDbCache _dbCache;
        private readonly IProviderManager _providerManager;

        public ProductionManager(IDbCache dbCache, IProviderManager providerManager)
        {
            _dbCache = dbCache;
            _providerManager = providerManager;
        }

        public void CreateProductionOrder(IDemand demand, IDemandManager demandManager)
        {
            T_ProductionOrder productionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            productionOrder.DueTime = demand.GetDueTime();
            productionOrder.Article = demand.GetArticle();
            productionOrder.Name = $"ProductionOrder{demand.Id}";
            // connects this provider with table T_Provider
            productionOrder.Provider = new T_Provider();
            productionOrder.Quantity = demand.GetQuantity();

            // TODO: check following navigation properties are created
            /*List<T_ProductionOrderBom> productionOrderBoms = new List<T_ProductionOrderBom>();
            List<T_ProductionOrderOperation> productionOrderWorkSchedule = new List<T_ProductionOrderOperation>();
            List<T_ProductionOrderBom> prodProductionOrderBomChilds = new List<T_ProductionOrderBom>();*/

            ProcessArticleBoms(demand, demandManager, productionOrder);

            _providerManager.AddProvider(productionOrder);
            LOGGER.Debug("ProductionOrder created.");
        }

        private void ProcessArticleBoms(IDemand demand, IDemandManager demandManager,
            T_ProductionOrder productionOrder)
        {
            M_Article readArticle = _dbCache.M_ArticleGetById(demand.GetArticle().Id);
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                /*if (!AdjacencyList.ContainsKey(givenArticle.Entity.Id))
                {
                    AdjacencyList.Add(givenArticle.Entity.Id, new List<Node<M_Article>>());
                }*/

                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    T_ProductionOrderBom productionOrderBom = CreateProductionOrderBom(articleBom,
                        productionOrder, demandManager.GetHierarchyNumber());
                    demandManager.AddDemand(productionOrderBom);
                }
            }
        }

        private T_ProductionOrderBom CreateProductionOrderBom(M_ArticleBom articleBom,
            T_ProductionOrder productionOrder, int hierarchyNumber)
        {
            T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();

            productionOrderBom.Quantity = articleBom.Quantity;
            productionOrderBom.State = State.Created;
            productionOrderBom.ProductionOrderParent = productionOrder;

            productionOrderBom.ProductionOrderOperation =
                CreateProductionOrderBomOperation(articleBom, hierarchyNumber, productionOrder);

            return productionOrderBom;
        }

        private T_ProductionOrderOperation CreateProductionOrderBomOperation(
            M_ArticleBom articleBom, int hierarchyNumber, T_ProductionOrder productionOrder)
        {
            if (articleBom.ArticleChild.ToPurchase)
            {
                return null;
            }
            T_ProductionOrderOperation productionOrderOperation = new T_ProductionOrderOperation();
            productionOrderOperation.Name = articleBom.Operation.Name;
            productionOrderOperation.HierarchyNumber = articleBom.Operation.HierarchyNumber;
            productionOrderOperation.Duration = articleBom.Operation.Duration;
            productionOrderOperation.MachineTool = articleBom.Operation.MachineTool;
            productionOrderOperation.MachineGroup = articleBom.Operation.MachineGroup;
            productionOrderOperation.ProductionOrder = productionOrder;
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
    }
}