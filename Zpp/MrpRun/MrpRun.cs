using System.Collections.Generic;
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
        
        public static void runMrp()
        {
            DemandToProviderManager demandToProviderManager = new DemandToProviderManager();
            List<IDemand> demands = null;
                
            demandToProviderManager.orderByUrgency(demands);
            foreach (IDemand demand in demands)
            {
                
            }
        }
    }
}