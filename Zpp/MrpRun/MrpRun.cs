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
            ProductionManager productionManager = new ProductionManager();
            IProviderManager providerManager = new ProviderManagerSimple(dbCache);
            IDemandManager demandManager = new DemandManagerSimple(dbCache, providerManager);

            PurchaseManager purchaseManager = new PurchaseManager(dbCache, providerManager);

            // start

            // remove all DemandToProvider entries
            dbCache.T_DemandToProvidersRemoveAll();

            demandManager.orderDemandsByUrgency();
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
                            demandManager.addProviderForDemand(demand.Id, provider.Id);
                            isDemandSatisfied = true;
                            break;
                        }
                    }
                }

                if (!isDemandSatisfied)
                    // create provider for it
                {
                    LOGGER.Debug("Create a provider for article " + demand.GetArticle().Id + ":");
                    if (demand.GetArticle().ToBuild)
                    {
                        productionManager.createProductionOrder();
                    }
                    else if (demand.GetArticle().ToPurchase)
                    {
                        purchaseManager.createPurchaseOrderPart(demand);
                    }
                }
            }

            // finalize
            purchaseManager.closeOpenPurchaseOrders();
            dbCache.persistDbCache();
        }
    }
}