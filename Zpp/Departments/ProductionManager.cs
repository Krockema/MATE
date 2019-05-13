namespace Zpp
{
    public class ProductionManager
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        
        public void createProductionOrder()
        {
            LOGGER.Debug("ProductionOrder created.");
        }
    }
}