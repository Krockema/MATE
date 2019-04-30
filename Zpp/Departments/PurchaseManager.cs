using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Enums;

namespace Zpp
{
    public class PurchaseManager
    {
        private readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private readonly List<T_PurchaseOrderPart> _purchaseOrderParts = new List<T_PurchaseOrderPart>();
        private readonly List<T_PurchaseOrder> _purchaseOrders = new List<T_PurchaseOrder>();
        private T_PurchaseOrder _purchaseOrder =  new T_PurchaseOrder();
        
        // getters/setters
        public List<T_PurchaseOrder> PurchaseOrders => _purchaseOrders;

        public void createPurchaseOrderPart(M_Article article, int quantity)
        {
            T_PurchaseOrderPart purchaseOrderPart = new T_PurchaseOrderPart();
            _purchaseOrderParts.Add(purchaseOrderPart);
            
            purchaseOrderPart.PurchaseOrder = _purchaseOrder;
            purchaseOrderPart.Article = article;
            purchaseOrderPart.Quantity = quantity;
            purchaseOrderPart.State = State.Created;
            purchaseOrderPart.Provider = new T_Provider();
            
            LOGGER.Debug("PurchaseOrderPart created.");
        }

        /// <summary>
        /// State Start: empty _purchaseOrder, list with created _purchaseOrderParts
        /// Transition: add list _purchaseOrderParts to _purchaseOrder
        /// State End: list _purchaseOrders is extended by created purchaseOrder, list _purchaseOrderParts & _purchaseOrder is reset
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dueTime"></param>
        /// <param name="businessPartner"></param>
        public void createPurchaseOrder(string name, int dueTime, M_BusinessPartner businessPartner)
        {
            // fill _purchaseOrder
            _purchaseOrder.Name = name;
            _purchaseOrder.DueTime = dueTime;
            _purchaseOrder.BusinessPartner = businessPartner;
            _purchaseOrder.PurchaseOrderParts = new List<T_PurchaseOrderPart>(_purchaseOrderParts);
            
            // reset
            _purchaseOrderParts.Clear();
            _purchaseOrders.Add(_purchaseOrder);
            _purchaseOrder = new T_PurchaseOrder();
            
            LOGGER.Debug("PurchaseOrder created.");
        }
    }
}