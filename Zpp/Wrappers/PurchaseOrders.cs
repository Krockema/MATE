using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp
{
    /**
     * wraps the collection with all purchaseOrders
     */
    public class PurchaseOrders
    {
        private List<PurchaseOrder> _purchaseOrders;

        public PurchaseOrders()
        {
            
        }

        public PurchaseOrders(List<T_PurchaseOrder> purchaseOrders)
        {
            _purchaseOrders = new List<PurchaseOrder>();
            foreach (var purchaseOrder in purchaseOrders)
            {
                _purchaseOrders.Add(new PurchaseOrder(purchaseOrder));
                
            }
        }
        
        public List<T_PurchaseOrder> GetAllAsT_PurchaseOrder()
        {
            List<T_PurchaseOrder> productionOrderBoms = new List<T_PurchaseOrder>();
            foreach (var demand in _purchaseOrders)
            {
                productionOrderBoms.Add(demand.ToT_PurchaseOrder());
            }
            return productionOrderBoms;
        }

        public void Add(PurchaseOrder purchaseOrder)
        {
            _purchaseOrders.Add(purchaseOrder);
        }
    }
}