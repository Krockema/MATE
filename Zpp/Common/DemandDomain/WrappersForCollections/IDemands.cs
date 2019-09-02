using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Zpp.DbCache;
using Zpp.WrappersForCollections;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.DemandDomain.WrappersForCollections
{
    /**
     * wraps the collection with all demands
     */
    public interface IDemands: ICollectionWrapper<Demand>
    {
        // TODO: Use this interface instead of the implementor Demands directly
        
        List<IDemand> GetAllAsIDemand();

        List<T> GetAllAs<T>();

        void OrderDemandsByUrgency(IDbTransactionData dbTransactionData);
        
        HierarchyNumber GetHierarchyNumber();

        void Clear();

        /**
         * sums quantites of all demands
         */
        Quantity GetQuantityOfAll();

        Demand GetDemandById(Id id);

    }
}