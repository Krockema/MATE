using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.DataModel
{
    public class T_Demand : BaseEntity
    {
        public IDemand ToIDemand(ProductionDomainContext productionDomainContext,
            T_Demand t_demand)
        {
            IDemand iDemand = null;
            
            iDemand = productionDomainContext.CustomerOrderParts.Single(x =>
                x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return iDemand;
            }
            
            iDemand = productionDomainContext.ProductionOrderBoms.Single(x =>
                x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return iDemand;
            }
            
            iDemand = productionDomainContext.StockExchanges.Single(x =>
                x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return iDemand;
            }

            return null;
        }
    }
}