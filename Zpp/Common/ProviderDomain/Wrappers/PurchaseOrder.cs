using Master40.DB.DataModel;

namespace Zpp.Common.ProviderDomain.Wrappers
{
    /**
     * wraps T_PurchaseOrder
     */
    public class PurchaseOrder
    {
        private T_PurchaseOrder _purchaseOrder;

        public PurchaseOrder(T_PurchaseOrder purchaseOrder)
        {
            _purchaseOrder = purchaseOrder;
        }

        public PurchaseOrder()
        {
        }

        public T_PurchaseOrder ToT_PurchaseOrder()
        {
            return _purchaseOrder;
        }
    }
}