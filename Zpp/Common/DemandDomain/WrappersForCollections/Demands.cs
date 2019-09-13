using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Zpp.DbCache;
using Zpp.WrappersForCollections;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.DemandDomain.WrappersForCollections
{
    /**
     * wraps the collection with all demands, earlier named "DemandManager"
     */
    public class Demands : CollectionWrapperWithList<Demand>, IDemands
    {
        private readonly HierarchyNumber _hierarchyNumber;

        public Demands(List<Demand> list) : base(list)
        {
        }

        public Demands()
        {
        }

        public List<IDemand> GetAllAsIDemand()
        {
            List<IDemand> iDemands = new List<IDemand>();
            foreach (var iDemand in List)
            {
                iDemands.Add(iDemand.ToIDemand());
            }

            return iDemands;
        }
        
        public List<T> GetAllAs<T>()
        {
            List<T> productionOrderBoms = new List<T>();
            foreach (var demand in List)
            {
                productionOrderBoms.Add((T)demand.ToIDemand());
            }
            return productionOrderBoms;
        }
        

        public void OrderDemandsByUrgency(IDbTransactionData dbTransactionData)
        {
            // sort only, if there are more than one element
            if (List.Count > 1)
            {
                List.Sort((x, y) =>
                {
                    return x.GetDueTime(dbTransactionData).CompareTo(y.GetDueTime(dbTransactionData));
                });
            }
        }
        
        public HierarchyNumber GetHierarchyNumber()
        {
            return _hierarchyNumber;
        }

        public void Clear()
        {
            List.Clear();
        }

        public Quantity GetQuantityOfAll()
        {
            Quantity sumQuantity = Quantity.Null();
            foreach (var demand in List)
            {
                sumQuantity.IncrementBy(demand.GetQuantity());
            }

            return sumQuantity;
        }
        
        public Demand GetDemandById(Id id)
        {
            // performance: cache this in a dictionary
            foreach (var demand in List)
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