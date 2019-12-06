using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.DataLayer.impl.WrappersForCollections
{
    public sealed class CustomerOrders : CollectionWrapperWithStackSet<T_CustomerOrder>
    {
        public CustomerOrders(List<T_CustomerOrder> list)
        {
            AddAll(list);
        }

        public CustomerOrders()
        {
        }

        public List<T_CustomerOrder> GetAllAsTCustomerOrders()
        {
            List<T_CustomerOrder> customerOrders = new List<T_CustomerOrder>();
            foreach (var customerOrder in this.StackSet)
            {
                customerOrders.Add(customerOrder);
            }
            return customerOrders;
        }
    }
}