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
        
        public static void runMrp(List<IDemand> demands)
        {
            IDemandManager demandManager = null;
            demandManager.orderByUrgency(demands);
            for (IDemand demand in demands)
            {
                
            }
        }
    }
}