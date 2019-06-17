using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.DataModel
{
    public class T_Demand : BaseEntity
    {
        public IDemand ToIDemand(T_Demand t_demand, List<T_CustomerOrderPart> customerOrderParts,
            List<T_ProductionOrderBom> productionOrderBoms, List<T_StockExchange> stockExchanges)
        {
            IDemand iDemand = null;

            iDemand = customerOrderParts.Single(x => x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return iDemand;
            }

            iDemand = productionOrderBoms.Single(x => x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return iDemand;
            }

            iDemand = stockExchanges.Single(x => x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return iDemand;
            }

            return null;
        }

        public override string ToString()
        {
            return Id.ToString();
        }  
    }
}