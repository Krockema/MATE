using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Zpp.DataLayer.impl.WrappersForCollections;

namespace Zpp.DataLayer.impl.DemandDomain.WrappersForCollections
{
    /**
     * wraps the collection with all demands, earlier named "DemandManager"
     */
    public class Demands : CollectionWrapperWithStackSet<Demand>
    {
        public Demands()
        {
        }

        public List<IDemand> GetAllAsIDemand()
        {
            List<IDemand> iDemands = new List<IDemand>();
            foreach (var iDemand in StackSet)
            {
                iDemands.Add(iDemand.ToIDemand());
            }

            return iDemands;
        }
        
        public List<T> GetAllAs<T>()
        {
            List<T> productionOrderBoms = new List<T>();
            foreach (var demand in StackSet)
            {
                productionOrderBoms.Add((T)demand.ToIDemand());
            }
            return productionOrderBoms;
        }

        public Quantity GetQuantityOfAll()
        {
            Quantity sumQuantity = Quantity.Zero();
            foreach (var demand in StackSet)
            {
                sumQuantity.IncrementBy(demand.GetQuantity());
            }

            return sumQuantity;
        }
        
        public Demand GetDemandById(Id id)
        {
            // performance: cache this in a dictionary
            foreach (var demand in StackSet)
            {
                if (demand.GetId().Equals(id))
                {
                    return demand;
                }
            }

            return null;
        }
    }
}