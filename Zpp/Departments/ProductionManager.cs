using Master40.DB.Interfaces;

namespace Zpp
{
    public class ProductionManager
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        
        public void createProductionOrder(IDemand demand)
        {
            
            
            LOGGER.Debug("ProductionOrder created.");
        }
    }
}