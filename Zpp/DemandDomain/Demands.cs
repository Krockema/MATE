using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp.DemandDomain
{
    /**
     * wraps the collection with all demands
     */
    public class Demands : IDemands
    {
        protected List<Demand> _demands;

        public Demands()
        {
        }

        public Demands(List<Demand> demands)
        {
            _demands = demands;
        }

        public void Add(Demand demand)
        {
            _demands.Add(demand);
        }

        public void AddAll(Demands demands)
        {
            _demands.AddRange(demands.GetAll());
        }

        public List<Demand> GetAll()
        {
            return _demands;
        }

        public List<IDemand> GetAllAsIDemand()
        {
            List<IDemand> iDemands = new List<IDemand>();
            foreach (var iDemand in _demands)
            {
                iDemands.Add(iDemand.ToIDemand());
            }

            return iDemands;
        }
        
        public List<T> GetAllAs<T>()
        {
            List<T> productionOrderBoms = new List<T>();
            foreach (var demand in _demands)
            {
                productionOrderBoms.Add((T)demand.ToIDemand());
            }
            return productionOrderBoms;
        }
    }
}