using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.DemandDomain
{
    /**
     * wraps the collection with all stockExchangeDemands
     */
    public class StockExchangeDemands : Demands
    {
        public StockExchangeDemands(List<T_StockExchange> iDemands,IDbMasterDataCache dbMasterDataCache
            ) : base(ToDemands(iDemands, dbMasterDataCache))
        {
        }

        private static List<Demand> ToDemands(List<T_StockExchange> iDemands, IDbMasterDataCache dbMasterDataCache)
        {
            List<Demand> demands = new List<Demand>();
            foreach (var iDemand in iDemands)
            {
                if (iDemand.DemandId == null)
                {
                    continue;
                }
                demands.Add(new StockExchangeDemand(iDemand, dbMasterDataCache));
            }

            return demands;
        }
    }
}