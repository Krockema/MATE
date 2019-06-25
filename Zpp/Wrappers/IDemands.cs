using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
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
    }
}