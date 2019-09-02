using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.DbCache;

namespace Zpp.Common.DemandDomain.WrappersForCollections
{
    /**
     * wraps collection with all customerOrderParts
     */
    public class CustomerOrderParts : Demands
    {
        public CustomerOrderParts(List<T_CustomerOrderPart> iDemands,
            IDbMasterDataCache dbMasterDataCache) : base(ToDemands(iDemands, dbMasterDataCache))
        {
        }

        private static List<Demand> ToDemands(List<T_CustomerOrderPart> iDemands,
            IDbMasterDataCache dbMasterDataCache)
        {
            List<Demand> demands = new List<Demand>();
            foreach (var iDemand in iDemands)
            {
                demands.Add(new CustomerOrderPart(iDemand, dbMasterDataCache));
            }

            return demands;
        }
    }
}