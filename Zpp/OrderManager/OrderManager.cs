using System.Collections.Generic;
using Master40.DB.Models;

namespace Zpp.OrderManager
{
    public class OrderManager
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public OrderManager()
        {
            
        }

        public void order(Order order)
        {
            LOGGER.Info("Starting: Order " + order.Id);
        }
    }
}