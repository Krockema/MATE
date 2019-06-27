using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;


namespace Zpp.DemandDomain
{
    /**
     * wraps collection with all customerOrderParts
     */
    public class CustomerOrderParts : Demands
    {
        public CustomerOrderParts(List<T_CustomerOrderPart> iDemands,
            IDbCacheMasterData dbCacheMasterData) : base(ToDemands(iDemands, dbCacheMasterData))
        {
        }

        private static List<Demand> ToDemands(List<T_CustomerOrderPart> iDemands,
            IDbCacheMasterData dbCacheMasterData)
        {
            List<Demand> demands = new List<Demand>();
            foreach (var iDemand in iDemands)
            {
                demands.Add(new CustomerOrderPart(iDemand, dbCacheMasterData));
            }

            return demands;
        }
    }
}