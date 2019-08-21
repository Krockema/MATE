using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp
{
    /**
     * wraps the collection with all purchaseOrders
     */
    public class PurchaseOrders: CollectionWrapperWithList<PurchaseOrder>
    {
        public PurchaseOrders(List<PurchaseOrder> list) : base(list)
        {
        }

        public PurchaseOrders()
        {
        }

        public List<T_PurchaseOrder> GetAllAsT_PurchaseOrder()
        {
            List<T_PurchaseOrder> productionOrderBoms = new List<T_PurchaseOrder>();
            foreach (var demand in List)
            {
                productionOrderBoms.Add(demand.ToT_PurchaseOrder());
            }
            return productionOrderBoms;
        }
    }
}