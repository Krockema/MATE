using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.WrappersForCollections;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;

namespace Master40.SimulationMrp.impl.Confirmation.impl
{
    public static class ConfirmationCreator
    {
        public static void CreateConfirmations(SimulationInterval simulationInterval)
        {
            /*ISimulator simulator = new Simulator();
            simulator.ProcessCurrentInterval(simulationInterval, _orderGenerator);*/
            // --> (Martin's impl) does not work correctly, use trivial impl instead TODO: Just an info for you Martin

            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            IAggregator aggregator = ZppConfiguration.CacheManager.GetAggregator();


            // customerOrderParts: set finished if all childs are finished
            DemandToProviderGraph demandToProviderGraph = new DemandToProviderGraph();
            INodes rootNodes = demandToProviderGraph.GetRootNodes();
            foreach (var rootNode in rootNodes)
            {
                if (rootNode.GetEntity().GetType() == typeof(CustomerOrderPart))
                {
                    CustomerOrderPart customerOrderPart = (CustomerOrderPart) rootNode.GetEntity();
                    customerOrderPart.SetReadOnly();

                    bool allChildsAreFinished = true;
                    foreach (var stockExchangeProvider in aggregator.GetAllChildProvidersOf(
                        customerOrderPart))
                    {
                        if (stockExchangeProvider.IsFinished() == false)
                        {
                            allChildsAreFinished = false;
                            break;
                        }
                    }

                    if (allChildsAreFinished)
                    {
                        customerOrderPart.SetFinished();
                    }
                }
            }

            // no confirmations: some nodes has no state (PrO) or must be adapted differently (COPs)
            // confirmations only for: stockExchanges, purchaseOrderParts, operations
            Type[] typesToAdapt = new Type[]
            {
                typeof(StockExchangeDemand), typeof(StockExchangeProvider),
                typeof(PurchaseOrderPart), typeof(ProductionOrderOperation)
            };

            // operations
            AdaptState(dbTransactionData.ProductionOrderOperationGetAll(), simulationInterval,
                typesToAdapt);

            // demands
            AdaptState(dbTransactionData.DemandsGetAll(), simulationInterval, typesToAdapt);

            // provider
            AdaptState(dbTransactionData.ProvidersGetAll(), simulationInterval, typesToAdapt);
        }

        private static void AdaptState(IEnumerable<IScheduleNode> scheduleNodes,
            SimulationInterval simulationInterval, Type[] typesToAdapt)
        {
            foreach (var scheduleNode in scheduleNodes)
            {
                // everyEntity must set read-only (to avoid future changes besides delete)
                scheduleNode.SetReadOnly();
                
                if (typesToAdapt.Contains(scheduleNode.GetType()) == false)
                {
                    continue;
                }

                bool startIsWithinInterval =
                    simulationInterval.IsWithinInterval(scheduleNode.GetStartTimeBackward());
                DueTime endTime = scheduleNode.GetEndTimeBackward();
                bool endTimeIsWithinIntervalOrBefore =
                    simulationInterval.IsWithinInterval(endTime) ||
                    simulationInterval.IsBeforeInterval(endTime);
                if (startIsWithinInterval)
                {
                    scheduleNode.SetInProgress();
                }

                if (endTimeIsWithinIntervalOrBefore)
                {
                    scheduleNode.SetFinished();
                }
            }
        }

        public static void CreateConfirmationsOld(SimulationInterval simulationInterval)
        {
            /*ISimulator simulator = new Simulator();
            simulator.ProcessCurrentInterval(simulationInterval, _orderGenerator);*/
            // --> does not work correctly, use trivial impl instead

            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            IAggregator aggregator = ZppConfiguration.CacheManager.GetAggregator();

            // stockExchanges, purchaseOrderParts, operations(use PrBom instead):
            // set in progress when startTime is within interval
            DemandOrProviders demandOrProvidersToSetInProgress = new DemandOrProviders();
            demandOrProvidersToSetInProgress.AddAll(
                aggregator.GetDemandsOrProvidersWhereStartTimeIsWithinInterval(simulationInterval,
                    new DemandOrProviders(dbTransactionData.PurchaseOrderPartGetAll())));
            demandOrProvidersToSetInProgress.AddAll(
                aggregator.GetDemandsOrProvidersWhereStartTimeIsWithinInterval(simulationInterval,
                    new DemandOrProviders(dbTransactionData.StockExchangeDemandsGetAll())));
            demandOrProvidersToSetInProgress.AddAll(
                aggregator.GetDemandsOrProvidersWhereStartTimeIsWithinInterval(simulationInterval,
                    new DemandOrProviders(dbTransactionData.StockExchangeProvidersGetAll())));
            demandOrProvidersToSetInProgress.AddAll(
                aggregator.GetDemandsOrProvidersWhereStartTimeIsWithinInterval(simulationInterval,
                    new DemandOrProviders(dbTransactionData.ProductionOrderBomGetAll())));

            foreach (var demandOrProvider in demandOrProvidersToSetInProgress)
            {
                demandOrProvider.SetInProgress();
                demandOrProvider.SetReadOnly();
            }

            // stockExchanges, purchaseOrderParts, operations(use PrBom instead):
            // set finished when endTime is within interval
            DemandOrProviders demandOrProvidersToSetFinished = new DemandOrProviders();
            demandOrProvidersToSetFinished.AddAll(
                aggregator.GetDemandsOrProvidersWhereEndTimeIsWithinIntervalOrBefore(
                    simulationInterval,
                    new DemandOrProviders(dbTransactionData.PurchaseOrderPartGetAll())));
            demandOrProvidersToSetFinished.AddAll(
                aggregator.GetDemandsOrProvidersWhereEndTimeIsWithinIntervalOrBefore(
                    simulationInterval,
                    new DemandOrProviders(dbTransactionData.StockExchangeDemandsGetAll())));
            demandOrProvidersToSetFinished.AddAll(
                aggregator.GetDemandsOrProvidersWhereEndTimeIsWithinIntervalOrBefore(
                    simulationInterval,
                    new DemandOrProviders(dbTransactionData.StockExchangeProvidersGetAll())));
            demandOrProvidersToSetFinished.AddAll(
                aggregator.GetDemandsOrProvidersWhereEndTimeIsWithinIntervalOrBefore(
                    simulationInterval,
                    new DemandOrProviders(dbTransactionData.ProductionOrderBomGetAll())));
            foreach (var demandOrProvider in demandOrProvidersToSetFinished)
            {
                demandOrProvider.SetFinished();
                demandOrProvider.SetReadOnly();
            }

            // customerOrderParts: set finished if all childs are finished
            DemandToProviderGraph demandToProviderGraph = new DemandToProviderGraph();
            INodes rootNodes = demandToProviderGraph.GetRootNodes();
            foreach (var rootNode in rootNodes)
            {
                if (rootNode.GetEntity().GetType() == typeof(CustomerOrderPart))
                {
                    CustomerOrderPart customerOrderPart = (CustomerOrderPart) rootNode.GetEntity();
                    customerOrderPart.SetReadOnly();

                    bool allChildsAreFinished = true;
                    foreach (var stockExchangeProvider in aggregator.GetAllChildProvidersOf(
                        customerOrderPart))
                    {
                        if (stockExchangeProvider.IsFinished() == false)
                        {
                            allChildsAreFinished = false;
                            break;
                        }
                    }

                    if (allChildsAreFinished)
                    {
                        customerOrderPart.SetFinished();
                    }
                }
            }

            // set operations readonly
            foreach (var operation in dbTransactionData.ProductionOrderOperationGetAll())
            {
                operation.SetReadOnly();
            }

            // set productionOrders readonly
            foreach (var productionOrder in dbTransactionData.ProductionOrderGetAll())
            {
                productionOrder.SetReadOnly();
            }

            // future SEs are still not readonly, set it so
            foreach (var stockExchangeDemand in dbTransactionData.StockExchangeDemandsGetAll())
            {
                stockExchangeDemand.SetReadOnly();
            }

            foreach (var stockExchangeProvider in dbTransactionData.StockExchangeProvidersGetAll())
            {
                stockExchangeProvider.SetReadOnly();
            }
        }
    }
}