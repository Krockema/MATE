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

        public ProductionManager(IDbCache dbCache)
        {
            _dbCache = dbCache;
        }
        
        public void createProductionOrder(IDemand demand, IDemandManager demandManager)
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

            processArticleBoms(demand, demandManager, productionOrder, demandManager.GetHierarchyNumber());
            
            LOGGER.Debug("ProductionOrder created.");
        }

        private void processArticleBoms(IDemand demand, IDemandManager demandManager, T_ProductionOrder productionOrder, int hierarchyNumber)
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
                    T_ProductionOrderBom productionOrderBom = createDemand();
                    demandManager.AddDemand(productionOrderBom);
                    T_ProductionOrderBom productionOrderBom = new T_ProductionOrderBom();
                    
                    productionOrderBom.Quantity = articleBom.Quantity;
                    productionOrderBom.State = State.Created;
                    productionOrderBom.ProductionOrderParent = productionOrder;
                    // ProductionOrderOperation --> nein, denn hier ist productionOrderBom ein Demand, wann dann? ("sie ist beides")
                    T_ProductionOrderOperation productionOrderOperation = new T_ProductionOrderOperation();
                    productionOrderBom.ProductionOrderOperation = productionOrderOperation;
                    productionOrderOperation.Name = articleBom.Operation.Name;
                    if (hierarchyNumber != articleBom.Operation.HierarchyNumber)
                    {
                        LOGGER.Error("Given hierarchyNumber is not matching HierarchyNumber of ArticleBomOperation!");
                    }
                    productionOrderOperation.HierarchyNumber = hierarchyNumber;
                    productionOrderOperation.Duration = articleBom.Operation.Duration;
                    productionOrderOperation.MachineTool = articleBom.Operation.MachineTool;
                    productionOrderOperation.MachineGroup = articleBom.Operation.MachineGroup;
                    productionOrderOperation.ProductionOrder = productionOrder;
                    // TODO: external Algo needed
                    // productionOrderOperation.Machine, Start, End, StartBackward, EndBackward,
                    // StartForward, EndForward, ActivitySlack, WorkTimeWithParents,
                    // StartSimulation, EndSimulation, DurationSimulation, ProducingState
                    
                }
            }
        }

        private T_ProductionOrderBom createDemand()
        {
            return null;
        }

    }
}