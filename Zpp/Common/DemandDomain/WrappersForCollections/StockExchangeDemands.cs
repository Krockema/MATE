using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.DbCache;

namespace Zpp.Common.DemandDomain.WrappersForCollections
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
                if (iDemand.StockExchangeType.Equals(StockExchangeType.Provider))
                {
                    continue;
                }
                demands.Add(new StockExchangeDemand(iDemand, dbMasterDataCache));
            }

            return demands;
        }
    }
}