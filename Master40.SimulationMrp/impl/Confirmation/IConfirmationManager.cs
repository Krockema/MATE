using Master40.DB.Data.Helper.Types;

namespace Master40.SimulationMrp.impl.Confirmation
{
    public interface IConfirmationManager
    {
        /**
         * Adapts the states of operations, customerOrders, stockExchanges, purchaseOrderParts
         */
        void CreateConfirmations(SimulationInterval simulationInterval);
        
        /**
         * Consult the documentation in diploma thesis of Pascal Schumann
         */
        void ApplyConfirmations();
    }
}