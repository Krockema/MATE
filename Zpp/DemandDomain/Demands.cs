using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Zpp.DemandDomain
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
                iDemands.Add(iDemand.GetIDemand());
            }

            return iDemands;
        }
        
        public List<T> GetAllAs<T>()
        {
            List<T> productionOrderBoms = new List<T>();
            foreach (var demand in List)
            {
                productionOrderBoms.Add((T)demand.GetIDemand());
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
    }
}