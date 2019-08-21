using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Priority_Queue;
using Priority_Queue_Example;
using Zpp.DemandDomain;
using Zpp.MachineDomain;
using Zpp.ProductionDomain;
using Zpp.ProviderDomain;
using Zpp.PurchaseDomain;
using Zpp.SchedulingDomain;
using Zpp.StockDomain;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp
{
    public static class MrpRun
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        // articleNode.Entity.ArticleType.Name.Equals(ArticleType.ASSEMBLY)

        /**
         * Only at start the demands are customerOrders
         */
        public static void RunMrp(ProductionDomainContext productionDomainContext)
        {
            // init data structures
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(productionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(productionDomainContext, dbMasterDataCache);

            // start

            // remove all DemandToProvider entries
            dbTransactionData.DemandToProvidersRemoveAll();

            ProcessDbDemands(dbTransactionData, dbMasterDataCache.T_CustomerOrderPartGetAll(),
                dbMasterDataCache);
        }

        /**
         * - save providers
         * - save dependingDemands
         */
        public static void ProcessProvidingResponse(Response response,
            IProviderManager providerManager, StockManager stockManager,
            IDbTransactionData dbTransactionData, Demand demand)
        {
            if (response == null)
            {
                return;
            }

            if (response.GetDemandToProviders() != null)
            {
                foreach (var demandToProvider in response.GetDemandToProviders())
                {
                    if (demandToProvider.GetDemandId().Equals(demand.GetId()) == false)
                    {
                        throw new MrpRunException(
                            "This demandToProvider does not fit to given demand.");
                    }

                    providerManager.AddDemandToProvider(demandToProvider);

                    if (response.GetProviders() != null)
                    {
                        Provider provider = response.GetProviders()
                            .GetProviderById(demandToProvider.GetProviderId());
                        if (provider != null)
                        {
                            stockManager.AdaptStock(provider, dbTransactionData);
                            providerManager.AddProvider(response.GetDemandId(), provider,
                                demandToProvider.GetQuantity());
                        }
                    }
                }
            }
        }

        private static IDemands ProcessNextDemand(IDbTransactionData dbTransactionData,
            Demand demand, IDbMasterDataCache dbMasterDataCache, IProvidingManager orderManager,
            StockManager stockManager, IProviderManager providerManager,
            IProvidingManager providingManager)
        {
            Response response;

            // SE:I --> satisfy by orders (PuOP/PrOBom)
            if (demand.GetType() == typeof(StockExchangeDemand))
            {
                response = orderManager.Satisfy(demand, demand.GetQuantity(), dbTransactionData);

                ProcessProvidingResponse(response, providerManager, stockManager, dbTransactionData,
                    demand);
            }
            // COP or PrOB --> satisfy by SE:W
            else
            {
                // satisfy by existing provider
                response = providingManager.Satisfy(demand, demand.GetQuantity(),
                    dbTransactionData);
                ProcessProvidingResponse(response, providerManager, stockManager, dbTransactionData,
                    demand);

                if (response.IsSatisfied() == false)
                {
                    response = stockManager.Satisfy(demand, response.GetRemainingQuantity(),
                        dbTransactionData);

                    ProcessProvidingResponse(response, providerManager, stockManager,
                        dbTransactionData, demand);
                }
            }

            if (response.GetRemainingQuantity().IsNull() == false)
            {
                throw new MrpRunException(
                    $"'{demand}' was NOT satisfied: remaining is {response.GetRemainingQuantity()}");
            }

            return providerManager.GetNextDemands();
        }


        private static void ProcessDbDemands(IDbTransactionData dbTransactionData,
            IDemands dbDemands, IDbMasterDataCache dbMasterDataCache)
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
            IProvidingManager providingManager = (IProvidingManager) providerManager;

            IProvidingManager orderManager = new OrderManager(dbMasterDataCache);
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
                    providerManager, providingManager);
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
            
            // write data to dbTransactionData
            globalStockManager.AdaptStock(stockManager);
            dbTransactionData.DemandsAddAll(finalAllDemands);
            dbTransactionData.ProvidersAddAll(providerManager.GetProviders());
            dbTransactionData.SetProviderManager(providerManager);
            
            // job shop scheduling
            MachineManager.JobSchedulingWithGifflerThompsonAsZaepfel(dbTransactionData,dbMasterDataCache, new PriorityRule());

            // persisting data
            dbTransactionData.PersistDbCache();

            LOGGER.Info("MrpRun done.");
        }
    }
}