using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.SimulationMrp.impl.CustomerOrder
{
    public interface ICustomerOrderCreator
    {
        /**
         * Exact order generating
         */
        void CreateCustomerOrders(SimulationInterval interval, Quantity customerOrderQuantity);
        
    }
}