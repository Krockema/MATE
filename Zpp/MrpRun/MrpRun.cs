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
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            // start

            // remove all DemandToProvider entries
            dbTransactionData.DemandToProvidersRemoveAll();


                ProcessDbDemands(dbTransactionData,
                    dbMasterDataCache.T_CustomerOrderPartGetAll());

                return new Plan(dbTransactionData.DemandsGetAll(), dbTransactionData.ProvidersGetAll(),
                dbTransactionData.DemandToProviderGetAll(), dbTransactionData, dbMasterDataCache);
        }

        private static void ProcessDbDemands(IDbTransactionData dbTransactionData,
            IDemands dbDemands)
        {
            // init
            IProviders providers = new Providers();
            IDemands finalAllDemands = new Demands();
            IDemandToProvidersMap demandToProvidersMap = new DemandToProvidersMap();
            IProviderToDemandsMap providerToDemandsMap = new ProviderToDemandsMap();

            foreach (var oneDbDemand in dbDemands.GetAll())
            {
                
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
                firstLevelDemandManager.Add(oneDbDemand);
                levelDemandManagers.Add(firstLevelDemandManager);


                while (true)
                {
                    IDemands currentDemandManager = levelDemandManagers[0];
                    currentDemandManager.OrderDemandsByUrgency();
                    // add new level for next creating demands (evolving tree of demands)
                    hierarchyNumber.increment();
                    IDemands nextDemandManager = new Demands(hierarchyNumber);
                    levelDemandManagers.Add(nextDemandManager);
                    // demands in currentDemandManager are not allowed to be expanded,
                    // nextDemandManager must be used for this
                    currentDemandManager.Lock();

                    foreach (Demand demand in currentDemandManager.GetAll())
                    {
                        IProviders providersOfDemand = demand.Satisfy(demandToProvidersMap,
                            dbTransactionData);
                        if (providersOfDemand.Any() == false)
                        {
                            throw new MrpRunException($"No provider were created for {demand}.");
                        }

                        demandToProvidersMap.AddProvidersForDemand(demand, providersOfDemand);
                        providers.AddAll(providersOfDemand);
                        // performance: replace any by directly Adding
                        if (providersOfDemand.AnyDependingDemands())
                        {
                            IProviderToDemandsMap dependingDemands = providersOfDemand.GetAllDependingDemandsAsMap(); 
                            nextDemandManager.AddAll(dependingDemands.GetAllDemands());
                            providerToDemandsMap.AddAll(providersOfDemand.GetAllDependingDemandsAsMap());
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
            }

            dbTransactionData.ProvidersAddAll(providers);
            dbTransactionData.DemandsAddAll(finalAllDemands);
            dbTransactionData.DemandToProviderAddAll(demandToProvidersMap);
            dbTransactionData.ProviderToDemandAddAll(providerToDemandsMap);
            dbTransactionData.PersistDbCache(demandToProvidersMap);
        }
    }
}