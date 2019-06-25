using System.Collections.Generic;
using Master40.DB.Interfaces;
using ZppForPrimitives;

namespace Zpp.DemandDomain
{
    /**
     * wraps the collection with all demands
     */
    public interface IDemands
    {
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
        void LockDemandsList();
    }
}