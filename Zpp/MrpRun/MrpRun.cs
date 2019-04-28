using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.ModelExtensions;
using Zpp.Utils;

namespace Zpp
{
    public static class MrpRun
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        
        // articleNode.Entity.ArticleType.Name.Equals(ArticleType.ASSEMBLY)
        
        public static void runMrp(List<Demand> demands)
        {
            IDemandToProviderManager demandToProviderManager = null;
            demandToProviderManager.orderByUrgency(demands);
            foreach (Demand demand in demands)
            {
                
            }
        }
    }
}