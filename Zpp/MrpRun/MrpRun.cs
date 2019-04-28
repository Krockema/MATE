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
            DemandToProviderManager demandToProviderManager = new DemandToProviderManager();
            List<IDemand> demands = null;
            List<IProvider> providers = null;
                
            // remove all DemandToProvider entries
            productionDomainContext.DemandToProviders.RemoveRange(productionDomainContext.DemandToProviders);
            productionDomainContext.SaveChanges();
            
            demandToProviderManager.orderByUrgency(demands);
            foreach (IDemand demand in demands)
            {
                foreach (IProvider provider in providers)
                {
                   // does a provider in time exists?
                }
            }
        }
    }
}