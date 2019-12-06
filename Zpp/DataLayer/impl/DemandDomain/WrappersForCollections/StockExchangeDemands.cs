using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;

namespace Zpp.DataLayer.impl.DemandDomain.WrappersForCollections
{
    /**
     * wraps the collection with all stockExchangeDemands
     */
    public sealed class StockExchangeDemands : Demands
    {
        public StockExchangeDemands(List<T_StockExchange> iDemands
            )
        {
            AddAll(ToDemands(iDemands));
        }

        private static List<Demand> ToDemands(List<T_StockExchange> iDemands)
        {
            List<Demand> demands = new List<Demand>();
            foreach (var iDemand in iDemands)
            {
                if (iDemand.StockExchangeType.Equals(StockExchangeType.Provider))
                {
                    continue;
                }
                demands.Add(new StockExchangeDemand(iDemand));
            }

            return demands;
        }
    }
}