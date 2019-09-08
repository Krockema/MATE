using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Xunit;
using Zpp.Common.DemandDomain;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Common.ProviderDomain;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;
using Zpp.MrpRun.MachineManagement;
using Zpp.OrderGraph;
using Zpp.Test.Configuration;
using Zpp.WrappersForPrimitives;

namespace Zpp.Test.Integration_Tests
{
    public class TestScheduling : AbstractTest
    {
        public TestScheduling() : base(false)
        {
        }

        private void InitThisTest(string testConfiguration)
        {
            InitTestScenario(testConfiguration);

            MrpRun.MrpRun.RunMrp(ProductionDomainContext);
        }


        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_CONCURRENT_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_SEQUENTIALLY_LOTSIZE_2)]
        // [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestBackwardScheduling(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            foreach (var productionOrderOperation in dbTransactionData
                .ProductionOrderOperationGetAll())
            {
                Assert.True(productionOrderOperation.GetValue().EndBackward != null,
                    $"EndBackward of operation ({productionOrderOperation} is not scheduled.)");
                Assert.True(productionOrderOperation.GetValue().StartBackward != null,
                    $"StartBackward of operation ({productionOrderOperation} is not scheduled.)");
            }
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_CONCURRENT_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_SEQUENTIALLY_LOTSIZE_2)]
        // [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestForwardScheduling(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            foreach (var productionOrderOperation in dbTransactionData
                .ProductionOrderOperationGetAll())
            {
                T_ProductionOrderOperation tProductionOrderOperation =
                    productionOrderOperation.GetValue();
                if (tProductionOrderOperation.StartBackward < 0)
                {
                    Assert.True(
                        tProductionOrderOperation.StartForward != null &&
                        tProductionOrderOperation.EndForward != null,
                        $"Operation ({tProductionOrderOperation}) is not scheduled forward.");
                    Assert.True(
                        tProductionOrderOperation.StartForward >= 0 &&
                        tProductionOrderOperation.EndForward >= 0,
                        "Forward schedule times of operation ({productionOrderOperation}) are negative.");
                }
            }

            List<DueTime> dueTimes = new List<DueTime>();
            foreach (var demand in dbTransactionData.DemandsGetAll().GetAll())
            {
                dueTimes.Add(demand.GetDueTime(dbTransactionData));
                Assert.True(demand.GetDueTime(dbTransactionData).GetValue() >= 0,
                    $"DueTime of demand ({demand}) is negative.");
            }

            foreach (var provider in dbTransactionData.ProvidersGetAll().GetAll())
            {
                dueTimes.Add(provider.GetDueTime(dbTransactionData));
                Assert.True(provider.GetDueTime(dbTransactionData).GetValue() >= 0,
                    $"DueTime of provider ({provider}) is negative.");
            }
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_1_LOT_ORDER_QUANTITY)]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_CONCURRENT_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_SEQUENTIALLY_LOTSIZE_2)]
        // [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestJobShopScheduling(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            foreach (var productionOrderOperation in dbTransactionData
                .ProductionOrderOperationGetAll())
            {
                T_ProductionOrderOperation tProductionOrderOperation =
                    productionOrderOperation.GetValue();
                Assert.True(tProductionOrderOperation.Start != tProductionOrderOperation.End,
                    $"{productionOrderOperation} was not scheduled.");
                Assert.True(tProductionOrderOperation.MachineId != null,
                    $"{productionOrderOperation} was not scheduled.");
                Assert.True(
                    tProductionOrderOperation.Start >= tProductionOrderOperation.StartBackward,
                    "The startTime for producing cannot be earlier than estimated by backwards scheduling.");
                Assert.True(tProductionOrderOperation.End >= tProductionOrderOperation.EndBackward,
                    "The endTime for producing cannot be earlier than estimated by backwards scheduling.");
            }
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_1_LOT_ORDER_QUANTITY)]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_CONCURRENT_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_SEQUENTIALLY_LOTSIZE_2)]
        // [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestPredecessorNodeTimeIsGreaterOrEqualInDemandToProviderGraph(
            string testConfigurationFileName)
        {
            // init
            InitThisTest(testConfigurationFileName);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            IDirectedGraph<INode> demandToProviderGraph =
                new DemandToProviderDirectedGraph(dbTransactionData);

            // start

            INodes allLeafs = demandToProviderGraph.GetLeafNodes();

            foreach (var leaf in allLeafs)
            {
                INodes predecessorNodes = demandToProviderGraph.GetPredecessorNodes(leaf);

                ValidatePredecessorNodeTimeIsGreaterOrEqual(predecessorNodes, leaf, dbTransactionData,
                    demandToProviderGraph);
            }
        }

        private void ValidatePredecessorNodeTimeIsGreaterOrEqual(INodes predecessorNodes,
            INode lastNode, IDbTransactionData dbTransactionData,
            IDirectedGraph<INode> demandToProviderGraph)
        {
            if (predecessorNodes == null || predecessorNodes.Any() == false)
            {
                return;
            }

            foreach (var predecessorNode in predecessorNodes)
            {
                if (predecessorNode.GetEntity().GetNodeType().Equals(NodeType.Demand))
                {
                    Demand currentDemand = (Demand) predecessorNode.GetEntity();

                    DueTime lastDueTime =
                        ((Provider) lastNode.GetEntity()).GetDueTime(dbTransactionData);
                    if (currentDemand.GetType() != typeof(CustomerOrderPart))
                    {
                        Assert.True(currentDemand.GetDueTime(dbTransactionData)
                            .IsGreaterThanOrEqualTo(lastDueTime), "PredecessorNodeTime cannot be smaller than node's time.");
                    }
                }
                else if (predecessorNode.GetNodeType().Equals(NodeType.Provider))
                {
                    Provider currentProvider = (Provider) predecessorNode.GetEntity();

                    DueTime lastDueTime =
                        ((Demand) lastNode.GetEntity()).GetDueTime(dbTransactionData);
                    Assert.True(currentProvider.GetDueTime(dbTransactionData)
                        .IsGreaterThanOrEqualTo(lastDueTime), "PredecessorNodeTime cannot be smaller than node's time.");
                }

                INodes newPredecessorNodes =
                    demandToProviderGraph.GetPredecessorNodes(predecessorNode);

                ValidatePredecessorNodeTimeIsGreaterOrEqual(newPredecessorNodes, predecessorNode,
                    dbTransactionData, demandToProviderGraph);
            }
        }
    }
}