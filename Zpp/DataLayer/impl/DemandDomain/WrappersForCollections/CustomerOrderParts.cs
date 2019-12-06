using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;

namespace Zpp.DataLayer.impl.DemandDomain.WrappersForCollections
{
    /**
     * wraps collection with all customerOrderParts
     */
    public sealed class CustomerOrderParts : Demands
    {
        public CustomerOrderParts(List<T_CustomerOrderPart> iDemands
            )
        {
            AddAll(ToDemands(iDemands));
        }

        public CustomerOrderParts()
        {
        }

        private static List<Demand> ToDemands(List<T_CustomerOrderPart> iDemands
            )
        {
            List<Demand> demands = new List<Demand>();
            foreach (var iDemand in iDemands)
            {
                demands.Add(new CustomerOrderPart(iDemand));
            }

            return demands;
        }
    }
}