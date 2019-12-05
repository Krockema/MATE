using System;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Priority_Queue;
using Zpp.Common.DemandDomain;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.Common.ProviderDomain;
using Zpp.DbCache;
using Zpp.Mrp.MachineManagement;
using Zpp.Mrp.NodeManagement;
using Zpp.Mrp.StockManagement;
using Zpp.Utils;
using Zpp.Utils.Queue;

namespace Zpp.Mrp
{
    public static class MrpRun
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        // articleNode.Entity.ArticleType.Name.Equals(ArticleType.ASSEMBLY)

        /**
         * Only at start the demands are customerOrders
         */
        public static void Start(ProductionDomainContext productionDomainContext)
        {
            // init data structures
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(productionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(productionDomainContext, dbMasterDataCache);

            // start

            // remove all DemandToProvider entries
            dbTransactionData.DemandToProvidersRemoveAll();

            ProcessDbDemands(dbTransactionData, dbMasterDataCache.T_CustomerOrderPartGetAll(),
                dbMasterDataCache, 0);
        }

        /**
         * - save providers
         * - save dependingDemands
         */
        public static void ProcessProvidingResponse(ResponseWithProviders responseWithProviders,
            IProviderManager providerManager, StockManager stockManager,
            IDbTransactionData dbTransactionData, Demand demand,
            IOpenDemandManager openDemandManager)
        {
            if (responseWithProviders == null)
            {
                return;
            }

            if (responseWithProviders.GetDemandToProviders() != null)
            {
                foreach (var demandToProvider in responseWithProviders.GetDemandToProviders())
                {
                    if (demandToProvider.GetDemandId().Equals(demand.GetId()) == false)
                    {
                        throw new MrpRunException(
                            "This demandToProvider does not fit to given demand.");
                    }

                    providerManager.AddDemandToProvider(demandToProvider);

                    if (responseWithProviders.GetProviders() != null)
                    {
                        Provider provider = responseWithProviders.GetProviders()
                            .GetProviderById(demandToProvider.GetProviderId());
                        if (provider != null)
                        {
                            stockManager.AdaptStock(provider, dbTransactionData, openDemandManager);
                            providerManager.AddProvider(responseWithProviders.GetDemandId(),
                                provider, demandToProvider.GetQuantity());
                        }
                    }
                }
            }
        }

        private static IDemands ProcessNextDemand(IDbTransactionData dbTransactionData,
            Demand demand, IDbMasterDataCache dbMasterDataCache, IProvidingManager orderManager,
            StockManager stockManager, IProviderManager providerManager,
            IOpenDemandManager openDemandManager)
        {
            ResponseWithProviders responseWithProviders;

            // SE:I --> satisfy by orders (PuOP/PrOBom)
            if (demand.GetType() == typeof(StockExchangeDemand))
            {
                responseWithProviders = orderManager.Satisfy(demand,
                    demand.GetQuantity(), dbTransactionData);

                ProcessProvidingResponse(responseWithProviders, providerManager, stockManager,
                    dbTransactionData, demand, openDemandManager);
            }
            // COP or PrOB --> satisfy by SE:W
            else
            {
                responseWithProviders = stockManager.Satisfy(demand,
                    demand.GetQuantity(), dbTransactionData);

                ProcessProvidingResponse(responseWithProviders, providerManager, stockManager,
                    dbTransactionData, demand, openDemandManager);
            }

            if (responseWithProviders.GetRemainingQuantity().IsNull() == false)
            {
                throw new MrpRunException(
                    $"'{demand}' was NOT satisfied: remaining is {responseWithProviders.GetRemainingQuantity()}");
            }

            return providerManager.GetNextDemands();
        }


        private static void ProcessDbDemands(IDbTransactionData dbTransactionData,
            IDemands dbDemands, IDbMasterDataCache dbMasterDataCache, int count)
        {
            // init
            IDemands finalAllDemands = new Demands();
            int MAX_DEMANDS_IN_QUEUE = 100000;

            FastPriorityQueue<DemandQueueNode> demandQueue =
                new FastPriorityQueue<DemandQueueNode>(MAX_DEMANDS_IN_QUEUE);

            StockManager globalStockManager =
                new StockManager(dbMasterDataCache.M_StockGetAll(), dbMasterDataCache);

            StockManager stockManager = new StockManager(globalStockManager, dbMasterDataCache);
            IProviderManager providerManager = new ProviderManager(dbTransactionData);

            IProvidingManager orderManager = new OrderManager(dbMasterDataCache);

            IOpenDemandManager openDemandManager = new OpenDemandManager();

            foreach (var demand in dbDemands.GetAll())
            {
                demandQueue.Enqueue(new DemandQueueNode(demand),
                    demand.GetDueTime(dbTransactionData).GetValue());
            }

            while (demandQueue.Count != 0)
            {
                DemandQueueNode firstDemandInQueue = demandQueue.Dequeue();

                IDemands nextDemands = ProcessNextDemand(dbTransactionData,
                    firstDemandInQueue.GetDemand(), dbMasterDataCache, orderManager, stockManager,
                    providerManager, openDemandManager);
                if (nextDemands != null)
                {
                    finalAllDemands.AddAll(nextDemands);
                    foreach (var demand in nextDemands.GetAll())
                    {
                        demandQueue.Enqueue(new DemandQueueNode(demand),
                            demand.GetDueTime(dbTransactionData).GetValue());
                    }
                }
            }
            /*
            // forward scheduling
            DueTime minDueTime = ForwardScheduler.FindMinDueTime(finalAllDemands,
                providerManager.GetProviders(), dbTransactionData);
            if (minDueTime.GetValue() < 0)
            {
                T_CustomerOrderPart thisCustomerOrderPart =
                    (T_CustomerOrderPart) oneCustomerOrderPart.GetIDemand();
                thisCustomerOrderPart.CustomerOrder.DueTime += Math.Abs(minDueTime.GetValue());
                ProcessNextCustomerOrderPart(dbTransactionData, oneCustomerOrderPart,
                    dbMasterDataCache, globalStockManager);
                return;
            }
            */


            // forward scheduling
            // TODO: remove this once forward scheduling is implemented
            // TODO 2: in forward scheduling, min must be calculuted by demand & provider,
            // not only providers, since operations are on PrOBom (which are demands)
            int min = 0;
            foreach (var provider in providerManager.GetProviders())
            {
                int start = provider.GetStartTime(dbTransactionData).GetValue();
                if (start < min)
                {
                    min = start;
                }
            }


            if (min < 0)
            {
                foreach (var dbDemand in dbDemands)
                {
                    if (dbDemand.GetType() == typeof(CustomerOrderPart))
                    {
                        T_CustomerOrderPart customerOrderPart =
                            ((T_CustomerOrderPart) ((CustomerOrderPart) dbDemand).ToIDemand());
                        customerOrderPart.CustomerOrder.DueTime =
                            customerOrderPart.CustomerOrder.DueTime + Math.Abs(min);
                    }
                }

                ProcessDbDemands(dbTransactionData, dbDemands, dbMasterDataCache, count++);
            }


            // persisting data
            if (count == 0)
                // it_s the first run, only do following here,
                // avoids executing this twice (else latest in forward scheduling recursion would also execute this)
            {
                // write data to dbTransactionData
                globalStockManager.AdaptStock(stockManager);
                dbTransactionData.DemandsAddAll(finalAllDemands);
                dbTransactionData.ProvidersAddAll(providerManager.GetProviders());
                dbTransactionData.SetProviderManager(providerManager);

                // job shop scheduling
                MachineManager.JobSchedulingWithGifflerThompsonAsZaepfel(dbTransactionData,
                    dbMasterDataCache, new PriorityRule());

                dbTransactionData.PersistDbCache();

                LOGGER.Info("MrpRun done.");
            }
        }
    }
}