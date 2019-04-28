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
            DemandToProviderManager demandToProviderManager = new DemandToProviderManager(productionDomainContext);
            List<IDemand> demands =
                demandToProviderManager.ToIDemands(productionDomainContext.Demands.ToList());
            List<IProvider> providers =
                demandToProviderManager.ToIProviders(
                    productionDomainContext.Providers.ToList());
            Dictionary<int, IDemand> demandsAsDictionary = demandToProviderManager.ToIDemandsAsDictionary();
            Dictionary<int, IProvider> providersAsDictionary = demandToProviderManager.ToIProvidersAsDictionary(providers);
               
            // start
            
            // remove all DemandToProvider entries
            productionDomainContext.DemandToProviders.RemoveRange(productionDomainContext.DemandToProviders);
            productionDomainContext.SaveChanges();
            
            demandToProviderManager.orderDemandsByUrgency(demands);
            foreach (IDemand demand in demands)
            {
                foreach (IProvider provider in providers)
                {
                   // does a provider in time exists?
                   if (demand.GetDueTime() < provider.GetAvailabilityTime() && false) // TODO
                   {
                       
                   }
                }
            }
        }
    }
}