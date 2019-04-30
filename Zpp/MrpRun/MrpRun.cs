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
            List<IDemand> demands =
                demandToProviderManager.ToIDemands(productionDomainContext.Demands.ToList());
            List<IProvider> providers =
                demandToProviderManager.ToIProviders(
                    productionDomainContext.Providers.ToList());
            Dictionary<int, IDemand> demandsAsDictionary =
                demandToProviderManager.ToIDemandsAsDictionary(demands);
            Dictionary<int, IProvider> providersAsDictionary =
                demandToProviderManager.ToIProvidersAsDictionary(providers);
            // use 2 dicts for demandToProvider to get O(1) in both ways
            Dictionary<int, int> demandToProvider = new Dictionary<int, int>();
            Dictionary<int, int> providerToDemand = new Dictionary<int, int>();
            // managers
            ProductionManager productionManager =
                new ProductionManager();

            PurchaseManager purchaseManager =
                new PurchaseManager();

            // start

            // remove all DemandToProvider entries
            productionDomainContext.DemandToProviders.RemoveRange(productionDomainContext
                .DemandToProviders);
            productionDomainContext.SaveChanges();

            demandToProviderManager.orderDemandsByUrgency(demands);
            foreach (IDemand demand in demands)
            {
                bool isDemandSatisfied = false;

                foreach (IProvider provider in providers)
                {
                    // does a provider in time exists?
                    if (demand.GetArticle().Id.Equals(provider.GetArticle().Id) &&
                        demand.GetDueTime() < provider.GetAvailabilityTime()) // TODO
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
                    // TODO: create provider
                    if (demand.GetArticle().ToBuild)
                    {
                        demandToProviderManager.createProductionOrder();
                    }
                    else if (demand.GetArticle().ToPurchase)
                    {
                        purchaseManager.createPurchaseOrderPart();
                    }
                }
            }
        }
    }
}