using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Master40.DB.Nominal;
using Zpp;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.OpenDemand;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrappersForCollections;
using Zpp.Util;
using Zpp.Util.StackSet;

namespace Master40.SimulationMrp.impl.Confirmation.impl
{
    public static class ConfirmationAppliance
    {
        public static void ApplyConfirmations()
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            IAggregator aggregator = ZppConfiguration.CacheManager.GetAggregator();

            // ProductionOrder: 3 Zustände siehe DA
            Providers copyOfProductionOrders = new Providers();
            copyOfProductionOrders.AddAll(dbTransactionData.ProductionOrderGetAll());

            foreach (var provider in copyOfProductionOrders)
            {
                ProductionOrder productionOrder = (ProductionOrder) provider;
                State state = productionOrder.DetermineProductionOrderState();
                switch (state)
                {
                    case State.Created:
                        HandleProductionOrderIsInStateCreated(productionOrder, aggregator,
                            dbTransactionData);
                        break;
                    case State.InProgress:
                        HandleProductionOrderIsInProgress();
                        break;
                    case State.Finished:
                        HandleProductionOrderIsFinished(productionOrder, aggregator,
                            dbTransactionData);
                        break;
                    default: throw new MrpRunException("This state is not expected.");
                }
            }


            // Entferne alle Pfeile auf StockExchangeProvider zeigend
            // Entferne alle Pfeile von StockExchangeDemands weg zeigend
            //  --> übrig beleiben Pfeile zwischen StockExchangeProvider und StockExchangeDemands
            RemoveArrowsBelowStockExchangeDemandsAndAboveStockExchangeProviders(dbTransactionData,
                aggregator);

            /*
                foreach sed in beendeten und geschlossenen StockExchangeDemands
                    Archiviere sed und seine parents (StockExchangeProvider) 
                      und die Pfeile dazwischen                
             */
            ArchiveClosedStockExchangeDemandsAndItsParents(dbTransactionData, aggregator);

            // this can be the case of stockExchangeProvider had multiple child stockExchangeDemands
            ArchiveStockExchangeProvidersWithoutChilds(dbTransactionData, aggregator);

            ArchiveFinishedCustomerOrderPartsAndDeleteTheirArrows(dbTransactionData, aggregator);
            ArchiveFinishedPurchaseOrderPartsAndDeleteTheirArrows(dbTransactionData, aggregator);

            ArchivedCustomerOrdersWithoutCustomerOrderParts();
            ArchivedPurchaseOrdersWithoutPurchaseOrderParts();
        }

        private static void ArchiveStockExchangeProvidersWithoutChilds(
            IDbTransactionData dbTransactionData, IAggregator aggregator)
        {
            Providers copyOfStockExchangeProviders = new Providers();
            copyOfStockExchangeProviders.AddAll(dbTransactionData.StockExchangeProvidersGetAll());
            foreach (var stockExchangeProvider in copyOfStockExchangeProviders)
            {
                Demands stockExchangeDemands =
                    aggregator.GetAllChildDemandsOf(stockExchangeProvider);
                if (stockExchangeDemands == null || stockExchangeDemands.Any() == false)
                {
                    ArchiveDemandOrProvider(stockExchangeProvider, dbTransactionData, aggregator,
                        false);
                }
            }
        }

        /**
         * // Entferne alle Pfeile auf StockExchangeProvider zeigend
            // Entferne alle Pfeile von StockExchangeDemands weg zeigend
            //  --> übrig beleiben Pfeile zwischen StockExchangeProvider und StockExchangeDemands
         */
        private static void RemoveArrowsBelowStockExchangeDemandsAndAboveStockExchangeProviders(
            IDbTransactionData dbTransactionData, IAggregator aggregator)
        {
            List<ILinkDemandAndProvider> stockExchangeLinks = new List<ILinkDemandAndProvider>();
            foreach (var stockExchangeDemand in dbTransactionData.StockExchangeDemandsGetAll())
            {
                List<ILinkDemandAndProvider> fromStockExchanges =
                    aggregator.GetArrowsFrom(stockExchangeDemand);
                if (fromStockExchanges != null)
                {
                    stockExchangeLinks.AddRange(fromStockExchanges);
                }
            }

            foreach (var stockExchangeProvider in dbTransactionData.StockExchangeProvidersGetAll())
            {
                List<ILinkDemandAndProvider> toStockExchanges =
                    aggregator.GetArrowsTo(stockExchangeProvider);
                if (toStockExchanges != null)
                {
                    stockExchangeLinks.AddRange(toStockExchanges);
                }
            }

            dbTransactionData.DeleteAllFrom(stockExchangeLinks);
        }

        /*
            foreach sed in beendeten und geschlossenen StockExchangeDemands
                Archiviere sed und seine parents (StockExchangeProvider) 
                  und die Pfeile dazwischen                
         */
        private static void ArchiveClosedStockExchangeDemandsAndItsParents(
            IDbTransactionData dbTransactionData, IAggregator aggregator)
        {
            IStackSet<IDemandOrProvider> stockExchangesToArchive =
                new StackSet<IDemandOrProvider>();
            foreach (var stockExchangeDemand in dbTransactionData.StockExchangeDemandsGetAll())
            {
                bool isOpen = OpenDemandManager.IsOpen((StockExchangeDemand) stockExchangeDemand);

                if (isOpen == false && stockExchangeDemand.IsFinished())
                {
                    stockExchangesToArchive.Push(stockExchangeDemand);

                    // parent (StockExchangeProviders)
                    Providers stockExchangeProviders =
                        aggregator.GetAllParentProvidersOf(stockExchangeDemand);
                    foreach (var stockExchangeProvider in stockExchangeProviders)
                    {
                        if (aggregator.GetAllChildDemandsOf(stockExchangeProvider).Count() == 1)
                        {
                            stockExchangesToArchive.Push(stockExchangeProvider);
                        }
                        else
                        {
                            // stockExchangeProvider must stay
                        }
                    }
                }
            }

            // archive collected stockexchanges
            foreach (var demandOrProviderToArchive in stockExchangesToArchive)
            {
                ArchiveDemandOrProvider(demandOrProviderToArchive, dbTransactionData, aggregator,
                    true);
            }
        }

        private static void ArchivedCustomerOrdersWithoutCustomerOrderParts()
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();


            Ids customerOrderIds = new Ids();
            foreach (var demand in dbTransactionData.CustomerOrderPartGetAll())
            {
                CustomerOrderPart customerOrderPart = (CustomerOrderPart) demand;
                customerOrderIds.Add(customerOrderPart.GetCustomerOrderId());
            }

            foreach (var customerOrder in dbTransactionData.CustomerOrderGetAll())
            {
                bool customerOrderHasNoCustomerOrderPart =
                    customerOrderIds.Contains(customerOrder.GetId()) == false;
                if (customerOrderHasNoCustomerOrderPart)
                {
                    ArchiveCustomerOrder(customerOrder.GetId());
                }
            }
        }

        private static void ArchivedPurchaseOrdersWithoutPurchaseOrderParts()
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();


            Ids purchaseOrderIds = new Ids();
            foreach (var demand in dbTransactionData.PurchaseOrderPartGetAll())
            {
                PurchaseOrderPart purchaseOrderPart = (PurchaseOrderPart) demand;
                purchaseOrderIds.Add(purchaseOrderPart.GetPurchaseOrderId());
            }

            foreach (var purchaseOrder in dbTransactionData.PurchaseOrderGetAll())
            {
                bool purchaseOrderHasNoPurchaseOrderParts =
                    purchaseOrderIds.Contains(purchaseOrder.GetId()) == false;
                if (purchaseOrderHasNoPurchaseOrderParts)
                {
                    ArchivePurchaseOrder(purchaseOrder);
                }
            }
        }

        /**
         * covers parent StockExchangeProvider(and its parent CustomerOrderPart if exist), child PurchaseOrderPart if exist
         * --> 3 types of subgraphs: Production, Purchase, Customer
         */
        private static List<IDemandOrProvider> GetItemsOfStockExchangeDemandSubGraph(
            StockExchangeDemand stockExchangeDemand, IDbTransactionData dbTransactionData,
            IAggregator aggregator, bool includeStockExchangeProviderHavingMultipleChilds)
        {
            List<IDemandOrProvider> items = new List<IDemandOrProvider>();
            items.Add(stockExchangeDemand);

            Providers stockExchangeProviders =
                aggregator.GetAllParentProvidersOf(stockExchangeDemand);
            foreach (var stockExchangeProvider in stockExchangeProviders)
            {
                Demands childsOfStockExchangeProvider =
                    aggregator.GetAllChildDemandsOf(stockExchangeProvider);
                if (includeStockExchangeProviderHavingMultipleChilds ||
                    childsOfStockExchangeProvider.Count() == 1)
                {
                    items.Add(stockExchangeProvider);
                    Demands customerOrderParts =
                        aggregator.GetAllParentDemandsOf(stockExchangeProvider);
                    if (customerOrderParts.Count() > 1)
                    {
                        throw new MrpRunException(
                            "A stockExchangeProvider can only have one parent.");
                    }

                    foreach (var customerOrderPart in customerOrderParts)
                    {
                        items.Add(customerOrderPart);
                    }
                }
            }

            Providers purchaseOrderParts = aggregator.GetAllChildProvidersOf(stockExchangeDemand);
            if (purchaseOrderParts.Count() > 1)
            {
                throw new MrpRunException("A stockExchangeDemand can only have one child.");
            }

            foreach (var purchaseOrderPart in purchaseOrderParts)
            {
                items.Add(purchaseOrderPart);
            }

            return items;
        }

        private static void ArchiveArrowsToAndFrom(IDemandOrProvider demandOrProvider,
            IDbTransactionData dbTransactionData, IDbTransactionData dbTransactionDataArchive,
            IAggregator aggregator)
        {
            List<ILinkDemandAndProvider> demandAndProviderLinks =
                aggregator.GetArrowsToAndFrom(demandOrProvider);
            foreach (var demandAndProviderLink in demandAndProviderLinks)
            {
                dbTransactionDataArchive.AddA(demandAndProviderLink);
                dbTransactionData.DeleteA(demandAndProviderLink);
            }
        }

        private static void ArchiveDemandOrProvider(IDemandOrProvider demandOrProvider,
            IDbTransactionData dbTransactionData, IAggregator aggregator, bool includeArrows)
        {
            if (demandOrProvider == null)
            {
                throw new MrpRunException("Given demandOrProvider cannot be null.");
            }

            IDbTransactionData dbTransactionDataArchive =
                ZppConfiguration.CacheManager.GetDbTransactionDataArchive();

            if (includeArrows)
            {
                ArchiveArrowsToAndFrom(demandOrProvider, dbTransactionData,
                    dbTransactionDataArchive, aggregator);
            }

            dbTransactionDataArchive.AddA(demandOrProvider);
            dbTransactionData.DeleteA(demandOrProvider);
        }


        /**
         * Subgraph of a productionOrder includes:
         * - parent (StockExchangeDemand) if includeStockExchanges true
         * - childs (ProductionOrderBoms)
         * - childs of childs (StockExchangeProvider) if includeStockExchanges true
         */
        private static List<IDemandOrProvider> CreateProductionOrderSubGraph(
            bool includeStockExchanges, ProductionOrder productionOrder, IAggregator aggregator)
        {
            List<IDemandOrProvider> demandOrProvidersOfProductionOrderSubGraph =
                new List<IDemandOrProvider>();
            demandOrProvidersOfProductionOrderSubGraph.Add(productionOrder);

            if (includeStockExchanges)
            {
                Demands stockExchangeDemands = aggregator.GetAllParentDemandsOf(productionOrder);
                if (stockExchangeDemands.Count() > 1)
                {
                    throw new MrpRunException(
                        "A productionOrder can only have one parentDemand (stockExchangeDemand).");
                }

                demandOrProvidersOfProductionOrderSubGraph.AddRange(stockExchangeDemands);
                foreach (var stockExchangeDemand in stockExchangeDemands)
                {
                    Providers parentStockExchangeProviders =
                        aggregator.GetAllParentProvidersOf(stockExchangeDemand);

                    foreach (var parentStockExchangeProvider in parentStockExchangeProviders)
                    {
                        if (aggregator.GetAllChildDemandsOf(parentStockExchangeProvider).Count() ==
                            1)
                        {
                            demandOrProvidersOfProductionOrderSubGraph.Add(
                                parentStockExchangeProvider);
                        }
                        else
                        {
                            // stockExchangeProvider must stay
                        }
                    }
                }
            }

            Demands productionOrderBoms = aggregator.GetAllChildDemandsOf(productionOrder);
            demandOrProvidersOfProductionOrderSubGraph.AddRange(productionOrderBoms);

            if (includeStockExchanges)
            {
                foreach (var productionOrderBom in productionOrderBoms)
                {
                    Providers stockExchangeProvider =
                        aggregator.GetAllChildProvidersOf(productionOrderBom);
                    if (stockExchangeProvider.Count() > 1)
                    {
                        throw new MrpRunException(
                            "A ProductionOrderBom can only have one childProvider (stockExchangeProvider).");
                    }

                    demandOrProvidersOfProductionOrderSubGraph.AddRange(stockExchangeProvider);
                }
            }

            return demandOrProvidersOfProductionOrderSubGraph;
        }

        private static void HandleProductionOrderIsInStateCreated(ProductionOrder productionOrder,
            IAggregator aggregator, IDbTransactionData dbTransactionData)
        {
            // delete all operations
            List<ProductionOrderOperation> operations =
                aggregator.GetProductionOrderOperationsOfProductionOrder(productionOrder);
            dbTransactionData.ProductionOrderOperationDeleteAll(operations);

            // collect entities and demandToProviders/providerToDemands to delete
            List<IDemandOrProvider> demandOrProvidersToDelete =
                CreateProductionOrderSubGraph(true, productionOrder, aggregator);

            // delete all collected entities
            IOpenDemandManager openDemandManager =
                ZppConfiguration.CacheManager.GetOpenDemandManager();
            foreach (var demandOrProvider in demandOrProvidersToDelete)
            {
                // don't forget to delete it from openDemands
                if (demandOrProvider.GetType() == typeof(StockExchangeDemand))
                {
                    if (openDemandManager.Contains((Demand) demandOrProvider))
                    {
                        openDemandManager.RemoveDemand((Demand) demandOrProvider);
                    }
                }

                List<ILinkDemandAndProvider> demandAndProviderLinks =
                    aggregator.GetArrowsToAndFrom(demandOrProvider);
                dbTransactionData.DeleteAllFrom(demandAndProviderLinks);

                dbTransactionData.DeleteA(demandOrProvider);
            }
        }

        private static void HandleProductionOrderIsInProgress()
        {
            // nothing to do here
        }

        private static void HandleProductionOrderIsFinished(ProductionOrder productionOrder,
            IAggregator aggregator, IDbTransactionData dbTransactionData)
        {
            IDbTransactionData dbTransactionDataArchive =
                ZppConfiguration.CacheManager.GetDbTransactionDataArchive();

            // archive operations
            ArchiveOperations(dbTransactionData, dbTransactionDataArchive, aggregator,
                productionOrder);

            // collect demands Or providers
            List<IDemandOrProvider> demandOrProvidersToArchive =
                CreateProductionOrderSubGraph(false, productionOrder, aggregator);

            // delete & archive all collected entities
            foreach (var demandOrProvider in demandOrProvidersToArchive)
            {
                // arrow outside mus be removed
                List<ILinkDemandAndProvider> demandAndProviderLinks;
                if (demandOrProvider.GetType() == typeof(ProductionOrder))
                {
                    // archive only link from ProductionOrder
                    demandAndProviderLinks = aggregator.GetArrowsFrom(demandOrProvider);
                    dbTransactionDataArchive.AddAllFrom(demandAndProviderLinks);
                    dbTransactionData.DeleteAllFrom(demandAndProviderLinks);
                    demandAndProviderLinks = aggregator.GetArrowsTo(demandOrProvider);
                    dbTransactionData.DeleteAllFrom(demandAndProviderLinks);
                }
                else if (demandOrProvider.GetType() == typeof(ProductionOrderBom))
                {
                    // archive only link to ProductionOrderBom
                    demandAndProviderLinks = aggregator.GetArrowsTo(demandOrProvider);
                    dbTransactionDataArchive.AddAllFrom(demandAndProviderLinks);
                    dbTransactionData.DeleteAllFrom(demandAndProviderLinks);
                    demandAndProviderLinks = aggregator.GetArrowsFrom(demandOrProvider);
                    dbTransactionData.DeleteAllFrom(demandAndProviderLinks);
                }
                else
                {
                    throw new MrpRunException("In this case not possible");
                }


                dbTransactionDataArchive.AddA(demandOrProvider);
                dbTransactionData.DeleteA(demandOrProvider);
            }
        }

        private static void ArchiveOperations(IDbTransactionData dbTransactionData,
            IDbTransactionData dbTransactionDataArchive, IAggregator aggregator,
            ProductionOrder productionOrder)
        {
            List<ProductionOrderOperation> operations =
                aggregator.GetProductionOrderOperationsOfProductionOrder(productionOrder);
            dbTransactionDataArchive.ProductionOrderOperationAddAll(operations);
            dbTransactionData.ProductionOrderOperationDeleteAll(operations);
        }

        private static void ArchiveFinishedCustomerOrderPartsAndDeleteTheirArrows(
            IDbTransactionData dbTransactionData, IAggregator aggregator)
        {
            Demands copyOfCustomerOrderParts = new Demands();
            copyOfCustomerOrderParts.AddAll(dbTransactionData.CustomerOrderPartGetAll());
            foreach (var demand in copyOfCustomerOrderParts)
            {
                CustomerOrderPart customerOrderPart = (CustomerOrderPart) demand;
                if (customerOrderPart.IsFinished())
                {
                    ArchiveCustomerOrder(customerOrderPart.GetCustomerOrderId());
                    // archive cop
                    List<ILinkDemandAndProvider> arrows =
                        aggregator.GetArrowsFrom(customerOrderPart);
                    dbTransactionData.DeleteAllFrom(arrows);
                    ArchiveDemandOrProvider(customerOrderPart, dbTransactionData, aggregator,
                        false);
                }
            }
        }

        private static void ArchiveFinishedPurchaseOrderPartsAndDeleteTheirArrows(
            IDbTransactionData dbTransactionData, IAggregator aggregator)
        {
            Providers copyOfPurchaseOrderParts = new Providers();
            copyOfPurchaseOrderParts.AddAll(dbTransactionData.PurchaseOrderPartGetAll());
            foreach (var purchaseOrderPart in copyOfPurchaseOrderParts)
            {
                if (purchaseOrderPart.IsFinished())
                {
                    // delete arrow
                    List<ILinkDemandAndProvider> arrows =
                        aggregator.GetArrowsFrom(purchaseOrderPart);
                    if (arrows != null)
                    {
                        dbTransactionData.DeleteAllFrom(arrows);
                    }

                    // archive purchaseOrderPart
                    ArchiveDemandOrProvider(purchaseOrderPart, dbTransactionData, aggregator,
                        false);
                }
            }
        }

        private static void ArchiveCustomerOrder(Id customerOrderId)
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            IDbTransactionData dbTransactionDataArchive =
                ZppConfiguration.CacheManager.GetDbTransactionDataArchive();
            T_CustomerOrder customerOrder = dbTransactionData.CustomerOrderGetById(customerOrderId);
            dbTransactionDataArchive.CustomerOrderAdd(customerOrder);
            dbTransactionData.CustomerOrderDelete(customerOrder);
        }

        private static void ArchivePurchaseOrder(T_PurchaseOrder purchaseOrder)
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            IDbTransactionData dbTransactionDataArchive =
                ZppConfiguration.CacheManager.GetDbTransactionDataArchive();
            dbTransactionDataArchive.PurchaseOrderAdd(purchaseOrder);
            dbTransactionData.PurchaseOrderDelete(purchaseOrder);
        }
    }
}