using Master40.SimulationMrp;
using Master40.XUnitTest.Zpp.Configuration;
using Xunit;
using Zpp;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.Mrp2.impl.Scheduling.impl;
using Zpp.Util.Graph;

namespace Master40.XUnitTest.Zpp.Integration_Tests.Verification
{
    public class VerifyBackwardForwardBackwardScheduling : AbstractVerification
    {
        [Theory]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_100_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_INTERVAL_20160_COP_100_LOTSIZE_2)]
        public void TestBackwardForwardBackwardScheduling(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.GetDbTransactionData();
            IDbTransactionData dbTransactionDataArchive =
                global::Zpp.ZppConfiguration.CacheManager.GetDbTransactionDataArchive();

            VerifyEndBackwardsMinusStartBackwardsEqualsDuration(dbTransactionData);
            VerifyEndBackwardsMinusStartBackwardsEqualsDuration(dbTransactionDataArchive);
        }

        private static void VerifyEndBackwardsMinusStartBackwardsEqualsDuration(
            IDbTransactionData dbTransactionData)
        {
            foreach (var productionOrderOperation in dbTransactionData
                .ProductionOrderOperationGetAll().GetAll())
            {
                int EndMinusStartBackwards =
                    productionOrderOperation.GetValue().EndBackward.GetValueOrDefault() -
                    productionOrderOperation.GetValue().StartBackward.GetValueOrDefault();
                Assert.True(
                    EndMinusStartBackwards.Equals(productionOrderOperation.GetValue().Duration),
                    "EndMinusStartBackwards does not equals the duration.");
            }
        }

        /**
         * Can only operate on one executed mrp2, simulation can not be used,
         * since confirmations would be applied and therefore no connection between ProductionOrderBoms
         * and its child StockExchangeProviders would exist anymore
         */
        [Theory]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_INTERVAL_20160_COP_100_LOTSIZE_2)]
        public void TestEveryOperationHasNeededMaterialAtStartBackward(
            string testConfigurationFileName)
        {
            InitTestScenario(testConfigurationFileName);

            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            // TODO: set to true once dbPersist() has an acceptable time
            zppSimulator.StartTestCycle(false);

            // TODO: replace this by ReloadTransactionData() once shouldPersist is enabled
            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.GetDbTransactionData();
            IAggregator aggregator = global::Zpp.ZppConfiguration.CacheManager.GetAggregator();

            foreach (var operation in dbTransactionData.ProductionOrderOperationGetAll())
            {
                Demands productionOrderBoms = aggregator.GetProductionOrderBomsBy(operation);
                foreach (var productionOrderBom in productionOrderBoms)
                {
                    foreach (var stockExchangeProvider in aggregator.GetAllChildProvidersOf(
                        productionOrderBom))
                    {
                        Assert.True(operation.GetStartTimeBackward().IsGreaterThanOrEqualTo(
                            stockExchangeProvider.GetEndTimeBackward()));
                    }
                }
            }
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_INTERVAL_20160_COP_100_LOTSIZE_2)]
        public void TestParentsDueTimeIsGreaterThanOrEqualToChildsDueTime(
            string testConfigurationFileName)
        {
            // init
            InitTestScenario(testConfigurationFileName);

            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            // TODO: set to true once dbPersist() has an acceptable time
            zppSimulator.StartTestCycle(false);

            // TODO: replace this by ReloadTransactionData() once shouldPersist is enabled
            global::Zpp.ZppConfiguration.CacheManager.GetDbTransactionData();
            
            OrderOperationGraph orderOperationGraph = new OrderOperationGraph();

            foreach (var graphNode in orderOperationGraph.GetNodes())
            {
                INode node = graphNode.GetNode();
                INodes successors = orderOperationGraph.GetSuccessorNodes(node);
                if (successors != null)
                {
                    foreach (var successor in successors)
                    {
                        if (node.GetEntity().GetType() == typeof(CustomerOrderPart))
                        {
                            continue;
                        }
                        Assert.True(
                            node.GetEntity().GetStartTimeBackward()
                                .IsGreaterThanOrEqualTo(successor.GetEntity().GetEndTimeBackward()),
                            "Parent's StartTimeBackward must be greater or equal to than child's EndTimeBackward.");
                    }
                }
            }
        }
    }
}