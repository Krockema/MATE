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

        public static void runMrp(ProductionDomainContext productionDomainContext)
        {
            // init data structures
            DemandToProviderManager demandToProviderManager =
                new DemandToProviderManager(productionDomainContext);
            // demand/provider


            // managers
            ProductionManager productionManager = new ProductionManager();

            PurchaseManager purchaseManager =
                new PurchaseManager(productionDomainContext, demandToProviderManager.Providers);

            // start

            // remove all DemandToProvider entries
            productionDomainContext.DemandToProviders.RemoveRange(productionDomainContext
                .DemandToProviders);

            demandToProviderManager.orderDemandsByUrgency(demandToProviderManager.Demands);
            foreach (IDemand demand in demandToProviderManager.Demands)
            {
                bool isDemandSatisfied = false;

                foreach (IProvider provider in demandToProviderManager.Providers)
                {
                    // does a provider in time exists?
                    if (demand.GetArticle().Id.Equals(provider.GetArticle().Id) &&
                        demand.GetDueTime() < provider.GetDueTime())
                    {
                        demandToProvider.Add(demand.Id, provider.Id);
                        providerToDemand.Add(provider.Id, demand.Id);
                        isDemandSatisfied = true;
                        break;
                    }
                }

                if (!isDemandSatisfied)
                {
                    LOGGER.Debug("Create a provider for article " + demand.GetArticle().Id + ":");
                    if (demand.GetArticle().ToBuild)
                    {
                        demandToProviderManager.createProductionOrder();
                    }
                    else if (demand.GetArticle().ToPurchase)
                    {
                        purchaseManager.createPurchaseOrderPart(demand);
                    }
                }
            }

            // finalize
            purchaseManager.closeOpenPurchaseOrders();
            productionDomainContext.SaveChanges();
        }
    }
}