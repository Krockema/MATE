using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
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

        private static void ProcessNextCustomerOrderPart(IDbTransactionData dbTransactionData,
            CustomerOrderPart oneCustomerOrderPart, IDbMasterDataCache dbMasterDataCache, StockManager globalStockManager)
        {
            // init
            IDemands finalAllDemands = new Demands();
            StockManager stockManager = new StockManager(globalStockManager);
            IProviderManager providerManager = new ProviderManager(stockManager);
            StockState stockState = new StockState();
            stockState.BackupStockState(dbMasterDataCache.M_StockGetAll());

            // Problem: while iterating demands sorted by dueTime (customerOrders) more demands will be
            // created (production/purchaseOrders) and these demands could be earlier than the current demand in loop
            // --> it's not possible to add these to the demandList even with Enumerators/Iterators
            // solution concept: create a new demandList per loop (one level)
            // and iterate over levels of evolving tree of demands
            // where every level is sorted by urgency & fix
            // and all created demands within a level is put to level below

            List<IDemands> levelDemandManagers = new List<IDemands>();
            // first level has the given oneDbDemand from database, while levels below are initially empty

            HierarchyNumber hierarchyNumber = new HierarchyNumber(1);
            IDemands firstLevelDemandManager = new Demands(hierarchyNumber);
            firstLevelDemandManager.Add(oneCustomerOrderPart);
            levelDemandManagers.Add(firstLevelDemandManager);

            while (true)
            {
                IDemands currentDemandManager = levelDemandManagers[0];
                currentDemandManager.OrderDemandsByUrgency(dbTransactionData);
                // add new level for next creating demands (evolving tree of demands)
                hierarchyNumber.increment();
                IDemands nextDemandManager = new Demands(hierarchyNumber);
                levelDemandManagers.Add(nextDemandManager);
                // demands in currentDemandManager are not allowed to be expanded,
                // nextDemandManager must be used for this
                currentDemandManager.Lock();

                foreach (Demand demand in currentDemandManager.GetAll())
                {
                    // SE:I
                    if (demand.GetType() == typeof(StockExchangeDemand))
                    {
                        demand.SatisfyStockExchangeDemand(providerManager, dbTransactionData);
                    }
                    // COP or PrOB
                    else
                    {
                        Quantity remainingQuantity = demand.SatisfyByStock(demand.GetQuantity(),
                            dbTransactionData, providerManager, demand);
                        if (remainingQuantity.IsNull() == false)
                        {
                            throw new MrpRunException($"COP/PrOB {demand} was NOT satisfied.");
                        }
                    }

                    Demands nextDemands = providerManager.GetNextDemands();
                    if (nextDemands != null && nextDemands.Any())
                    {
                        nextDemandManager.AddAll(nextDemands);
                    }
                }

                // final reorganizing
                levelDemandManagers.Remove(currentDemandManager);

                // break condition
                if (nextDemandManager.GetAll().Count == 0)
                {
                    break;
                }

                finalAllDemands.AddAll(nextDemandManager);
            }

            // MrpRun done

            // forward scheduling
            DueTime minDueTime = ForwardScheduler.FindMinDueTime(finalAllDemands,
                providerManager.GetProviders(), dbTransactionData);
            if (minDueTime.GetValue() < 0)
            {
                // reset stock.currents
                dbMasterDataCache.M_StockSetAll(stockState.ResetStockState());

                T_CustomerOrderPart thisCustomerOrderPart =
                    (T_CustomerOrderPart) oneCustomerOrderPart.GetIDemand();
                thisCustomerOrderPart.CustomerOrder.DueTime += Math.Abs(minDueTime.GetValue());
                ProcessNextCustomerOrderPart(dbTransactionData, oneCustomerOrderPart,
                    dbMasterDataCache, globalStockManager);
                return;
            }

            // persisting data
            globalStockManager.AdaptStock(stockManager);
            dbTransactionData.DemandsAddAll(finalAllDemands);
            dbTransactionData.ProvidersAddAll(providerManager.GetProviders());
            dbTransactionData.PersistDbCache(providerManager);
        }

        private static void ProcessDbDemands(IDbTransactionData dbTransactionData,
            IDemands dbDemands, IDbMasterDataCache dbMasterDataCache)
        {
            // init
            StockManager stockManager = new StockManager(dbMasterDataCache.M_StockGetAll());

            foreach (var oneDbDemand in dbDemands.GetAll())
            {
                if (oneDbDemand.GetType() == typeof(CustomerOrderPart))
                {
                    ProcessNextCustomerOrderPart(dbTransactionData, (CustomerOrderPart) oneDbDemand,
                        dbMasterDataCache, stockManager);
                }
            }

            LOGGER.Info("MrpRun done.");
        }
    }
}