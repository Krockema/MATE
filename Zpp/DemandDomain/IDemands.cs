using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Zpp.DemandDomain
{
    /**
     * wraps the collection with all demands
     */
    public interface IDemands
    {
        // TODO: Use this interface instead of the implementor Demands directly
        
        void Add(Demand demand);
        
        void AddAll(Demands demands);

        List<Demand> GetAll();
        
        List<IDemand> GetAllAsIDemand();

        List<T> GetAllAs<T>();
        
        void OrderDemandsByUrgency();
        
        HierarchyNumber GetHierarchyNumber();

        /// <summary>
        /// demandsList are not allowed to be expanded after this call
        /// </summary>
        void Lock();

        int Size();

        bool Any();

        void Clear();

        /**
         * sums quantites of all demands
         */
        Quantity GetQuantityOfAll();

    }
}