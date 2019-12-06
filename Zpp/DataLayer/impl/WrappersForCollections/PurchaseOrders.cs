using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DataLayer.impl.WrapperForEntities;

namespace Zpp.DataLayer.impl.WrappersForCollections
{
    /**
     * wraps the collection with all purchaseOrders
     */
    public sealed class PurchaseOrders: CollectionWrapperWithStackSet<PurchaseOrder>
    {
        public PurchaseOrders(List<PurchaseOrder> list) : base()
        {
            AddAll(list);
        }

        public PurchaseOrders()
        {
        }

        public List<T_PurchaseOrder> GetAllAsT_PurchaseOrder()
        {
            List<T_PurchaseOrder> productionOrderBoms = new List<T_PurchaseOrder>();
            foreach (var demand in StackSet)
            {
                productionOrderBoms.Add(demand.ToT_PurchaseOrder());
            }
            return productionOrderBoms;
        }
    }
}