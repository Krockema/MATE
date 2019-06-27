using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Zpp.DemandDomain;
using Zpp.DemandToProviderDomain;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.ModelExtensions;
using Zpp.Utils;
using Zpp;

namespace Zpp
{
    public static class MrpRun
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        // articleNode.Entity.ArticleType.Name.Equals(ArticleType.ASSEMBLY)

        /**
         * Only at start the demands are customerOrders
         */
        public static Plan RunMrp(ProductionDomainContext ProductionDomainContext)
        {
            // init data structures
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData = new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            
            // start

            // remove all DemandToProvider entries
            dbTransactionData.DemandToProvidersRemoveAll();
            
            foreach (var initialDemand in dbMasterDataCache.T_CustomerOrderPartGetAll())
            {
                ProcessDbDemand(dbTransactionData, new CustomerOrderPart(initialDemand, dbMasterDataCache));
            }

            return new Plan(dbTransactionData.DemandsGetAll(), dbTransactionData.ProvidersGetAll());
        }

        private static void ProcessDbDemand(IDbTransactionData dbTransactionData, Demand oneDbDemand)
        {
            // init
            Providers providers = new Providers();
            Demands finalAllDemands = new Demands();
            IDemandToProviders demandToProviders = new DemandToProviders();

            // Problem: while iterating demands sorted by dueTime (customerOrders) more demands will be
            // created (production/purchaseOrders) and these demands could be earlier than the current demand in loop
            // --> it's not possible to add these to the demandList even with Enumerators/Iterators
            // solution concept: create a new demandList per loop (one level)
            // and iterate over levels of evolving tree of demands
            // where every level is sorted by urgency & fix
            // and all created demands within a level is put to level below

            List<Demands> levelDemandManagers = new List<Demands>();
            // first level has the given oneDbDemand from database, while levels below are initially empty

            HierarchyNumber hierarchyNumber = new HierarchyNumber(1);
            Demands firstLevelDemandManager = new Demands(hierarchyNumber);
            firstLevelDemandManager.Add(oneDbDemand);
            levelDemandManagers.Add(firstLevelDemandManager);


            while (true)
            {
                Demands currentDemandManager = levelDemandManagers[0];
                currentDemandManager.OrderDemandsByUrgency();
                // add new level for next creating demands (evolving tree of demands)
                hierarchyNumber.increment();
                Demands nextDemandManager = new Demands(hierarchyNumber);
                levelDemandManagers.Add(nextDemandManager);
                // demands in currentDemandManager are not allowed to be expanded,
                // nextDemandManager must be used for this
                currentDemandManager.Lock();

                foreach (Demand demand in currentDemandManager.GetAll())
                {
                    // bool isDemandSatisfied = demandToProvider.IsSatisfied(demand); --> not needed here
                    Provider nonExhaustedProvider = 

                    if (!isDemandSatisfied)
                        // search/create provider for it
                    {
                        LOGGER.Debug(
                            $"Create a provider for article {demand}:");
                        Provider provider = demand.CreateProvider(dbTransactionData);
                        demandToProviders.SatisfyDemandWithProviders(demand, );
                        if (provider.AnyDemands())
                        {
                            nextDemandManager.AddAll(provider.GetDemands());    
                        }
                        providers.Add(provider);
                    }
                }

                // final reorganizing
                finalAllDemands.AddAll(currentDemandManager);
                levelDemandManagers.Remove(currentDemandManager);

                // break condition
                if (nextDemandManager.GetAll().Count == 0)
                {
                    break;
                }
            }
            
            dbTransactionData.ProvidersAddAll(providers);
            dbTransactionData.DemandsAddAll(finalAllDemands);
            dbTransactionData.PersistDbCache();
        }
    }
}