using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Interfaces;

namespace Master40.DB.DataModel
{
    public class T_Provider : BaseEntity
    {
        public IProvider ToIProvider(ProductionDomainContext productionDomainContext,
            T_Provider t_provider)
        {
            IProvider iProvider = null;
            
            iProvider = productionDomainContext.StockExchanges.Single(x =>
                x.Id == t_provider.Id);
            if (iProvider != null)
            {
                return iProvider;
            }
            
            iProvider = productionDomainContext.ProductionOrders.Single(x =>
                x.Id == t_provider.Id);
            if (iProvider != null)
            {
                return iProvider;
            }
            
            iProvider = productionDomainContext.PurchaseOrderParts.Single(x =>
                x.Id == t_provider.Id);
            if (iProvider != null)
            {
                return iProvider;
            }
            


            return null;
        }
    }
}