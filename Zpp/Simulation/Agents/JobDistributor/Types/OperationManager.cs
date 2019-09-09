using Master40.DB.Enums;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;
using Zpp.Mrp.MachineManagement;
using Zpp.Mrp.ProductionManagement.ProductionTypes;
using Zpp.OrderGraph;

namespace Zpp.Simulation.Agents.JobDistributor.Types
{
    public class OperationManager
    {
        private readonly IDbMasterDataCache _dbMasterDataCache;
        private readonly IAggregator _aggregator;
        private readonly IDbTransactionData _dbTransactionData;
        private readonly IDirectedGraph<INode> _productionOrderGraph;
        private readonly ProductionOrderOperationGraphs _productionOrderOperationGraphs;
        


        public OperationManager(IDbMasterDataCache dbMasterDataCache, 
                              IDbTransactionData dbTransactionData)
        {
            _dbTransactionData = dbTransactionData;
            _dbMasterDataCache = dbMasterDataCache;
            _aggregator = dbTransactionData.GetAggregator();
            _productionOrderGraph = new ProductionOrderDirectedGraph(_dbTransactionData, false);
            _productionOrderOperationGraphs = new ProductionOrderOperationGraphs();
            Init();
        }

        private void Init()
        {
            foreach (var productionOrder in _dbTransactionData.ProductionOrderGetAll())
            {
                IDirectedGraph<INode> productionOrderOperationGraph =
                    new ProductionOrderOperationDirectedGraph(_dbTransactionData,
                        (ProductionOrder)productionOrder);
                _productionOrderOperationGraphs.Add((ProductionOrder)productionOrder,
                    productionOrderOperationGraph);
            }
        }

        public void RemoveOperation(ProductionOrderOperation operation)
        {

            var productionOrder = operation.GetProductionOrder(_dbTransactionData);
            var productionOrderOperationGraph =
                (ProductionOrderOperationDirectedGraph)_productionOrderOperationGraphs[productionOrder];

            // prepare for next round
            productionOrderOperationGraph.RemoveNode(operation);
            productionOrderOperationGraph
                    // TODO Naming ?
                .RemoveProductionOrdersWithNoProductionOrderOperationsFromProductionOrderGraph(
                    _productionOrderGraph, productionOrder);
        }

        /// <summary>
        /// returns the mature cherry's
        /// </summary>
        /// <returns></returns>
        public IStackSet<ProductionOrderOperation> GetLeafs()
        {
            var productionOrderOperations
                    = MachineManager.CreateS(_productionOrderGraph, _productionOrderOperationGraphs);
            return productionOrderOperations;
        }

        public void WithdrawMaterialsFromStock(ProductionOrderOperation operation, long time)
        {
            var productionOrderBoms = _dbTransactionData.GetAggregator().GetAllProductionOrderBomsBy(operation);
            foreach (var productionOrderBom in productionOrderBoms)
            {
                var providers = _dbTransactionData.GetAggregator().GetAllChildProvidersOf(productionOrderBom);
                foreach (var provider in providers)
                {
                    var stockExchangeProvider = (StockExchangeProvider)provider;
                    var stockExchange = (T_StockExchange)stockExchangeProvider.ToIProvider();
                    stockExchange.State = State.Finished;
                    stockExchange.Time = (int)time;
                }
            }
        }

        public void InsertMaterialsIntoStock(ProductionOrderOperation operation, long time)
        {
            var productionOrderBom = _dbTransactionData.GetAggregator()
                .GetAnyProductionOrderBomByProductionOrderOperation(operation);


            var demands = _aggregator.GetAllParentDemandsOf(productionOrderBom.GetProductionOrder(_dbTransactionData));
            foreach (var demand in demands)
            {
                var stockExchangeProvider = (StockExchangeDemand) demand;
                var stockExchange = (T_StockExchange) stockExchangeProvider.ToIDemand();
                stockExchange.State = State.Finished;
                stockExchange.Time = (int) time;
            }
        }
    }
}