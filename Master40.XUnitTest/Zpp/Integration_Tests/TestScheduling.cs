using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.SimulationMrp;
using Master40.XUnitTest.Zpp.Configuration;
using Xunit;
using Zpp;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.Mrp2.impl.Scheduling.impl;
using Zpp.Util;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;
using Zpp.Util.StackSet;

namespace Master40.XUnitTest.Zpp.Integration_Tests
{
    public class TestScheduling : AbstractTest
    {
        public TestScheduling() : base(false)
        {
        }

        private void InitThisTest(string testConfiguration)
        {
            InitTestScenario(testConfiguration);

            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            zppSimulator.StartTestCycle();
        }


        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestBackwardScheduling(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);


            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.ReloadTransactionData();

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
        [InlineData(TestConfigurationFileNames.DESK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestForwardScheduling(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);


            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.ReloadTransactionData();

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
            foreach (var demand in dbTransactionData.DemandsGetAll())
            {
                dueTimes.Add(demand.GetStartTimeBackward());
                Assert.True(demand.GetStartTimeBackward().GetValue() >= 0,
                    $"DueTime of demand ({demand}) is negative.");
            }

            foreach (var provider in dbTransactionData.ProvidersGetAll())
            {
                dueTimes.Add(provider.GetStartTimeBackward());
                Assert.True(provider.GetStartTimeBackward().GetValue() >= 0,
                    $"DueTime of provider ({provider}) is negative.");
            }
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestJobShopScheduling(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.ReloadTransactionData();
            foreach (var productionOrderOperation in dbTransactionData
                .ProductionOrderOperationGetAll())
            {
                T_ProductionOrderOperation tProductionOrderOperation =
                    productionOrderOperation.GetValue();
                Assert.True(tProductionOrderOperation.Start != tProductionOrderOperation.End,
                    $"{productionOrderOperation} was not scheduled.");
                Assert.True(tProductionOrderOperation.ResourceId != null,
                    $"{productionOrderOperation} was not scheduled.");
                Assert.True(
                    tProductionOrderOperation.Start >= productionOrderOperation
                        .GetEarliestPossibleStartTime().GetValue(),
                    "A productionOrderOperation cannot start before its material is available.");
            }
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestParentsDueTimeIsGreaterThanOrEqualToChildsDueTime(
            string testConfigurationFileName)
        {
            // init
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.ReloadTransactionData();

            foreach (var demandToProvider in dbTransactionData.DemandToProviderGetAll())
            {
                Demand parentDemand =
                    dbTransactionData.DemandsGetById(demandToProvider.GetDemandId());
                if (parentDemand.GetType() == typeof(CustomerOrderPart))
                {
                    continue;
                }

                Provider childProvider =
                    dbTransactionData.ProvidersGetById(demandToProvider.GetProviderId());


                DueTime parentDueTime = parentDemand.GetStartTimeBackward();
                DueTime childDueTime = childProvider.GetEndTimeBackward();

                Assert.True(parentDueTime.IsGreaterThanOrEqualTo(childDueTime),
                    "ParentDemand's dueTime cannot be smaller than childProvider's dueTime.");
            }

            foreach (var providerToDemand in dbTransactionData.ProviderToDemandGetAll())
            {
                Provider parentProvider =
                    dbTransactionData.ProvidersGetById(providerToDemand.GetProviderId());
                Demand childDemand =
                    dbTransactionData.DemandsGetById(providerToDemand.GetDemandId());

                DueTime parentDueTime = parentProvider.GetStartTimeBackward();
                DueTime childDueTime = childDemand.GetEndTimeBackward();

                Assert.True(parentDueTime.IsGreaterThanOrEqualTo(childDueTime),
                    "ParentProvider's dueTime cannot be smaller than childDemand's dueTime.");
            }
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestBackwardSchedulingTransitionTimeForeachOperationIsCorrect(
            string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.ReloadTransactionData();

            foreach (var productionOrderBomAsDemand in dbTransactionData.ProductionOrderBomGetAll())
            {
                ProductionOrderBom productionOrderBom =
                    (ProductionOrderBom) productionOrderBomAsDemand;
                int expectedStartBackward = productionOrderBom.GetStartTimeBackward().GetValue() +
                                            TransitionTimer.GetTransitionTimeFactor() *
                                            productionOrderBom.GetDurationOfOperation().GetValue();
                int actualStartBackward = productionOrderBom.GetStartTimeOfOperation().GetValue();
                Assert.True(expectedStartBackward.Equals(actualStartBackward),
                    $"The transition time before operationStart is not correct: " +
                    $"expectedStartBackward: {expectedStartBackward}, actualStartBackward {actualStartBackward}");

                int expectedEndBackward = productionOrderBom.GetStartTimeOfOperation().GetValue() +
                                          productionOrderBom.GetDurationOfOperation().GetValue();
                int actualEndBackward = productionOrderBom.GetEndTimeBackward().GetValue();
                Assert.True(expectedEndBackward.Equals(actualEndBackward),
                    $"EndBackward is not correct: " +
                    $"expectedEndBackward: {expectedEndBackward}, actualEndBackward {actualEndBackward}");
            }
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestBackwardSchedulingTransitionTimeBetweenOperationsIsCorrect(
            string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.ReloadTransactionData();

            IDirectedGraph<INode> operationGraph =
                new OperationGraph(new OrderOperationGraph());

            IStackSet<INode> innerLeafs =
                operationGraph.GetLeafNodes().ToStackSet();
            IStackSet<INode> traversedNodes = new StackSet<INode>();
            foreach (var leaf in innerLeafs)
            {
                INodes predecessorNodesRecursive =
                    operationGraph.GetPredecessorNodesRecursive(leaf);
                IStackSet<ProductionOrderOperation> newPredecessorNodes =
                    new StackSet<ProductionOrderOperation>(
                        predecessorNodesRecursive.Select(x =>
                            (ProductionOrderOperation) x.GetEntity()));

                ProductionOrderOperation
                    lastOperation = (ProductionOrderOperation) leaf.GetEntity();
                ValidatePredecessorOperationsTransitionTimeIsCorrect(newPredecessorNodes,
                    lastOperation, operationGraph, traversedNodes);
                traversedNodes.Push(leaf);
            }

            int expectedTraversedOperationCount =
                new Stack<ProductionOrderOperation>(
                    dbTransactionData.ProductionOrderOperationGetAll()).Count();
            int actualTraversedOperationCount = traversedNodes.Count();

            Assert.True(actualTraversedOperationCount.Equals(expectedTraversedOperationCount),
                $"expectedTraversedOperationCount {expectedTraversedOperationCount} " +
                $"doesn't equal actualTraversedOperationCount {actualTraversedOperationCount}'");
        }

        /**
         * Bottom-Up-Traversal
         */
        private void ValidatePredecessorOperationsTransitionTimeIsCorrect(
            IStackSet<ProductionOrderOperation> predecessorOperations,
            ProductionOrderOperation lastOperation,
            IDirectedGraph<INode> operationGraph,
            IStackSet<INode> traversedOperations)
        {
            if (predecessorOperations == null)
            {
                return;
            }

            foreach (var currentPredecessor in predecessorOperations)
            {
                if (currentPredecessor.GetType() == typeof(ProductionOrderOperation))
                {
                    ProductionOrderOperation currentOperation = currentPredecessor;
                    traversedOperations.Push(new Node(currentPredecessor));

                    // transition time MUST be before the start of Operation
                    int expectedStartBackwardLowerLimit =
                        lastOperation.GetValue().EndBackward.GetValueOrDefault() +
                        TransitionTimer.GetTransitionTimeFactor() *
                        currentOperation.GetValue().Duration;
                    int actualStartBackward = currentOperation.GetValue().StartBackward
                        .GetValueOrDefault();
                    Assert.True(actualStartBackward >= expectedStartBackwardLowerLimit,
                        $"The transition time between the operations is not correct: " +
                        $"expectedStartBackward: {expectedStartBackwardLowerLimit}, actualStartBackward {actualStartBackward}");

                    INodes predecessorNodesRecursive =
                        operationGraph.GetPredecessorNodesRecursive(new Node(currentPredecessor));
                    if (predecessorNodesRecursive != null)
                    {
                        IStackSet<ProductionOrderOperation> newPredecessorNodes =
                            new StackSet<ProductionOrderOperation>(
                                predecessorNodesRecursive.Select(x =>
                                    (ProductionOrderOperation) x.GetEntity()));
                        ValidatePredecessorOperationsTransitionTimeIsCorrect(newPredecessorNodes,
                            currentOperation, operationGraph, traversedOperations);
                    }
                }
                else
                {
                    throw new MrpRunException(
                        "ProductionOrderToOperationGraph should only contain productionOrders/operations.");
                }
            }
        }
        
    }
}