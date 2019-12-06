using System.Collections.Generic;
using System.Linq;
using Master40.SimulationMrp;
using Master40.XUnitTest.Zpp.Configuration;
using Xunit;
using Zpp;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.Mrp2.impl.Scheduling.impl;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;

namespace Master40.XUnitTest.Zpp.Integration_Tests
{
    public class TestProductionOrderToOperationGraph : AbstractTest
    {
        public TestProductionOrderToOperationGraph() : base(initDefaultTestConfig: false)
        {
        }

        private void InitThisTest(string testConfiguration)
        {
            InitTestScenario(testConfiguration);

            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            zppSimulator.StartTestCycle();
        }

        [Theory]
        
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestGraphIsComplete(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.ReloadTransactionData();

            IDirectedGraph<INode> operationGraph =
                new OperationGraph(new OrderOperationGraph());

            IEnumerable<ProductionOrderOperation> productionOrderOperations =
                operationGraph.GetNodes().Select(x=>(ProductionOrderOperation)x.GetNode().GetEntity());
            foreach (var productionOrderOperation in dbTransactionData
                .ProductionOrderOperationGetAll())
            {
                Assert.True(productionOrderOperations.Contains(productionOrderOperation),
                    $"{productionOrderOperation} is missing.");
            }
        }
    }
}