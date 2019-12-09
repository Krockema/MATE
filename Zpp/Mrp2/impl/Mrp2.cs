using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper.Types;
using Zpp.DataLayer;
using Zpp.DataLayer.impl;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.Mrp2.impl.Mrp1;
using Zpp.Mrp2.impl.Scheduling;
using Zpp.Mrp2.impl.Scheduling.impl;
using Zpp.Mrp2.impl.Scheduling.impl.JobShopScheduler;
using Zpp.Util;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;
using Zpp.Util.Performance;

namespace Zpp.Mrp2.impl
{
    public class Mrp2 : IMrp2
    {
        private readonly IJobShopScheduler _jobShopScheduler = new JobShopScheduler();
        private readonly SimulationInterval _simulationInterval;

        public Mrp2(SimulationInterval simulationInterval)
        {
            _simulationInterval = simulationInterval;
        }

        private void ManufacturingResourcePlanning(Demands dbDemands)
        {
            if (dbDemands == null || dbDemands.Any() == false)
            {
                throw new MrpRunException(
                    "How could it happen, that no dbDemands are given to plan ?");
            }

            // MaterialRequirementsPlanning
            IMrp1 mrp1 = new Mrp1.impl.Mrp1(dbDemands);
            mrp1.StartMrp1();


            // BackwardForwardBackwardScheduling

            OrderOperationGraph orderOperationGraph = new OrderOperationGraph();

            ScheduleBackwardFirst(orderOperationGraph);
            AssertEveryDemandAndProviderIsScheduled();

            ScheduleForward(orderOperationGraph, _simulationInterval);
            ScheduleBackwardSecond(orderOperationGraph);

            // job shop scheduling
            JobShopScheduling(orderOperationGraph);
        }

        private void AssertEveryDemandAndProviderIsScheduled()
        {
            if (ZppConfiguration.IsInPerformanceMode)
            {
                return;
            }

            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            foreach (var demand in dbTransactionData.DemandsGetAll())
            {
                if (demand.GetStartTimeBackward() == null || demand.GetEndTimeBackward() == null)
                {
                    throw new MrpRunException($"A demand ({demand} must scheduled.)");
                }
            }

            foreach (var provider in dbTransactionData.ProvidersGetAll())
            {
                if (provider.GetStartTimeBackward() == null ||
                    provider.GetEndTimeBackward() == null)
                {
                    throw new MrpRunException($"A demand ({provider}) must scheduled.)");
                }
            }
        }

        private void AssertGraphsAreNotEmpty(OrderOperationGraph orderOperationGraph)
        {
            if (ZppConfiguration.IsInPerformanceMode)
            {
                return;
            }

            DemandToProviderGraph demandToProviderGraph = new DemandToProviderGraph();
            if (demandToProviderGraph.IsEmpty())
            {
                throw new MrpRunException("How could the demandToProviderGraph be empty ?");
            }

            if (orderOperationGraph.IsEmpty())
            {
                throw new MrpRunException("How could the orderOperationGraph be empty ?");
            }
        }

        private void ScheduleBackwardFirst(IDirectedGraph<INode> orderOperationGraph)
        {
            INodes rootNodes = new Nodes();
            foreach (var rootNode in orderOperationGraph.GetRootNodes())
            {
                if (rootNode.GetEntity().GetType() == typeof(CustomerOrderPart))
                {
                    rootNodes.Add(rootNode);
                }
            }

            IBackwardsScheduler backwardsScheduler =
                new BackwardScheduler(rootNodes.ToStack(), orderOperationGraph, true);
            backwardsScheduler.ScheduleBackward();
        }

        private void ScheduleBackwardSecond(IDirectedGraph<INode> orderOperationGraph)
        {
            INodes childRootNodes = new Nodes();
            foreach (var rootNode in orderOperationGraph.GetRootNodes())
            {
                if (rootNode.GetEntity().GetType() != typeof(CustomerOrderPart))
                {
                    continue;
                }

                Providers childProviders = ZppConfiguration.CacheManager.GetAggregator()
                    .GetAllChildProvidersOf((Demand) rootNode.GetEntity());
                if (childProviders == null || childProviders.Count() > 1)
                {
                    throw new MrpRunException(
                        "After Mrp1 a CustomerOrderPart MUST have exact one child.");
                }

                childRootNodes.AddAll(childProviders.ToNodes());
            }

            IBackwardsScheduler backwardsScheduler =
                new BackwardScheduler(childRootNodes.ToStack(), orderOperationGraph, false);
            backwardsScheduler.ScheduleBackward();
        }

        private void ScheduleBackward(Stack<INode> rootNodes,
            IDirectedGraph<INode> orderOperationGraph, bool clearOldTimes)
        {
            IBackwardsScheduler backwardsScheduler =
                new BackwardScheduler(rootNodes, orderOperationGraph, clearOldTimes);
            backwardsScheduler.ScheduleBackward();
        }

        private void ScheduleForward(OrderOperationGraph orderOperationGraph, SimulationInterval simulationInterval)
        {
            IForwardScheduler forwardScheduler = new ForwardScheduler(orderOperationGraph, simulationInterval);
            forwardScheduler.ScheduleForward();
        }

        private void JobShopScheduling(OrderOperationGraph orderOperationGraph)
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            IAggregator aggregator = ZppConfiguration.CacheManager.GetAggregator();

            // some validations
            if (dbTransactionData.ProductionOrderGetAll().Any() == false)
            {
                // no JobShopScheduling needed, all Demands were satisfied without the need for a productionOrder
                return;
            }

            foreach (var productionOrder in dbTransactionData.ProductionOrderGetAll())
            {
                List<ProductionOrderOperation> operations =
                    aggregator.GetProductionOrderOperationsOfProductionOrder(
                        (ProductionOrder) productionOrder);
                if (operations == null || operations.Any() == false)
                {
                    throw new MrpRunException(
                        "How could it happen, that a productionOrder without operations exists ?");
                }
            }

            // start !
            IDirectedGraph<INode> operationGraph =
                new OperationGraph(orderOperationGraph);
            _jobShopScheduler.ScheduleWithGifflerThompsonAsZaepfel(
                new PriorityRule(operationGraph), operationGraph);
        }

        public void StartMrp2()
        {
            // execute mrp2
            Demands unsatisfiedCustomerOrderParts = ZppConfiguration.CacheManager.GetAggregator()
                .GetUnsatisfiedCustomerOrderParts();
            ManufacturingResourcePlanning(unsatisfiedCustomerOrderParts);
        }
    }
}