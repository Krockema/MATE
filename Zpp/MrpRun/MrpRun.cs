using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.ModelExtensions;
using Zpp.Utils;

namespace Zpp
{
    public static class MrpRun
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        // articleNode.Entity.ArticleType.Name.Equals(ArticleType.ASSEMBLY)

        public static void runMrp(IDbCache dbCache)
        {
            // init data structures

            // managers
            IProviderManager providerManager = new ProviderManagerSimple(dbCache);
            ProductionManager productionManager = new ProductionManager(dbCache, providerManager);

            PurchaseManager purchaseManager = new PurchaseManager(dbCache, providerManager);

            // start

            // remove all DemandToProvider entries
            dbCache.T_DemandToProvidersRemoveAll();

            // Problem: while iterating demands sorted by dueTime (customerOrders) more demands will be
            // created (production/purchaseOrders) and these demands could be earlier than the current demand in loop
            // --> it's not possible to add these to the demandList even with Enumerators/Iterators
            // solution concept: create a new list per loop (one level)
            // and iterate over levels of evolving tree of demands
            // where every level is sorted by urgency & fix
            // and all created demands within a level is put to level below

            List<IDemandManager> levelDemandManagers = new List<IDemandManager>();
            // first level has the demands from database, while levels below are initially empty
            IDemandManager firstLevelDemandManager =
                new DemandManagerSimple(dbCache, providerManager);
            levelDemandManagers.Add(firstLevelDemandManager);
            int hierachyNumber = 1;

            using (IEnumerator<IDemandManager> enumerator = levelDemandManagers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IDemandManager demandManager = enumerator.Current;
                    demandManager.OrderDemandsByUrgency();
                    // add new level for next creating demands (evolving tree of demands)
                    hierachyNumber++;
                    levelDemandManagers.Add(new DemandManagerSimple(providerManager, hierachyNumber));
                    foreach (IDemand demand in demandManager.GetDemands())
                    {
                        bool isDemandSatisfied = false;

                        if (providerManager.GetProviders() != null)
                        {
                            foreach (IProvider provider in providerManager.GetProviders())
                            {
                                // does a provider in time exists?
                                if (demand.GetArticle().Id.Equals(provider.GetArticle().Id) &&
                                    demand.GetDueTime() < provider.GetDueTime())
                                {
                                    demandManager.AddProviderForDemand(demand.Id, provider.Id);
                                    isDemandSatisfied = true;
                                    break;
                                }
                            }
                        }

                        if (!isDemandSatisfied) 
                            // create provider for it
                        {
                            LOGGER.Debug("Create a provider for article " + demand.GetArticle().Id +
                                         ":");
                            if (demand.GetArticle().ToBuild)
                            {
                                productionManager.CreateProductionOrder(demand, demandManager);
                            }
                            else if (demand.GetArticle().ToPurchase)
                            {
                                purchaseManager.createPurchaseOrderPart(demand);
                            }
                        }
                    }
                }
            }

            // finalize
            purchaseManager.closeOpenPurchaseOrders();
            dbCache.persistDbCache();
        }
    }
}