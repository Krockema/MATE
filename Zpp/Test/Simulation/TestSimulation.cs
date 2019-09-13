using Xunit;
using Zpp.DbCache;
using Zpp.Mrp;
using Zpp.Simulation;
using Zpp.Simulation.Types;
using Zpp.WrappersForPrimitives;

namespace Zpp.Test.Simulation
{
    public class TestSimulation : AbstractTest
    {
        private readonly IDbMasterDataCache _dbMasterDataCache;
        private readonly IDbTransactionData _dbTransactionData;
        public TestSimulation() : base(initDefaultTestConfig: true)
        {
            MrpRun.Start(ProductionDomainContext);
            _dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            _dbTransactionData = new DbTransactionData(ProductionDomainContext, _dbMasterDataCache);

        }

        [Fact]
        public void TestSimulationWithResults()
        {
            var Simulator = new Simulator(_dbMasterDataCache, _dbTransactionData);
            var simulationInterval = new SimulationInterval(0, 300);
            Simulator.ProcessCurrentInterval(simulationInterval);
            _dbTransactionData.PersistDbCache();
        }

        [Fact(Skip = "Only for single Execution.")]
        public void ProvideStockExchanges()
        {
            var from = new DueTime(0);
            var to = new DueTime(1440);
            var stockExchanges = _dbTransactionData.GetAggregator().GetProvidersForInterval(from, to);
            // .GetAll StockExchangeProvidersGetAll().GetAll();
            foreach (var stockExchange in stockExchanges)
            {
                stockExchange.SetProvided(stockExchange.GetDueTime());
            }
            _dbTransactionData.PersistDbCache();
        }
    }
}