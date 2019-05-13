using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Interfaces;

namespace Master40.DB.DataModel
{
    public class T_Provider : BaseEntity
    {
        public IProvider ToIProvider(T_Provider t_provider, List<T_PurchaseOrderPart> purchaseOrderParts,
            List<T_ProductionOrder> productionOrders, List<T_StockExchange> stockExchanges)
        {
            IProvider iProvider = null;
            
            iProvider = stockExchanges.Single(x =>
                x.Id == t_provider.Id);
            if (iProvider != null)
            {
                return iProvider;
            }
            
            iProvider = productionOrders.Single(x =>
                x.Id == t_provider.Id);
            if (iProvider != null)
            {
                return iProvider;
            }
            
            iProvider = purchaseOrderParts.Single(x =>
                x.Id == t_provider.Id);
            if (iProvider != null)
            {
                return iProvider;
            }
            


            return null;
        }
    }
}