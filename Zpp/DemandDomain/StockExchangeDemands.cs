using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.DemandDomain
{
    /**
     * wraps the collection with all stockExchangeDemands
     */
    public class StockExchangeDemands : Demands
    {
        public StockExchangeDemands(List<T_StockExchange> iDemands,IDbCacheMasterData dbCacheMasterData
            ) : base(ToDemands(iDemands, dbCacheMasterData))
        {
        }

        private static List<Demand> ToDemands(List<T_StockExchange> iDemands, IDbCacheMasterData dbCacheMasterData)
        {
            List<Demand> demands = new List<Demand>();
            foreach (var iDemand in iDemands)
            {
                demands.Add(new StockExchangeDemand(iDemand, dbCacheMasterData));
            }

            return demands;
        }
    }
}