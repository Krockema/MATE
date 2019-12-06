using Master40.DB.DataModel;

namespace Zpp.DataLayer.impl.WrapperForEntities
{
    /**
     * wraps T_PurchaseOrder
     */
    public class CustomerOrder
    {
        private T_CustomerOrder _customerOrder;

        public CustomerOrder(T_CustomerOrder customerOrder)
        {
            _customerOrder = customerOrder;
        }

        public CustomerOrder()
        {
        }

        public T_CustomerOrder ToT_CustomerOrder()
        {
            return _customerOrder;

        }
    }
}
