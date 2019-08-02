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
    public class Demands : IDemands
    {
        protected readonly List<Demand> _demands = new List<Demand>();
        private bool _isDemandsListLocked = false;
        private readonly HierarchyNumber _hierarchyNumber;

        public Demands()
        {
        }

        public Demands(HierarchyNumber hierarchyNumber)
        {
            _hierarchyNumber = hierarchyNumber;
        }

        public Demands(List<Demand> demands)
        {
            if (demands == null)
            {
                throw new MrpRunException("Given list should not be null.");
            }
            // here is a copy created of given demands
            _demands = new List<Demand>();
            _demands.AddRange(demands);
        }

        public void Add(Demand demand)
        {
            if (_isDemandsListLocked)
            {
                throw new MrpRunException("Demands is locked, no demands can be added anymore.");
            }
            _demands.Add(demand);
        }

        public void AddAll(IDemands demands)
        {
            if (_isDemandsListLocked)
            {
                throw new MrpRunException("Demands is locked, no demands can be added anymore.");
            }

            if (demands == null || demands.GetAll() == null || demands.GetAll().Count.Equals(0))
            {
                throw new MrpRunException("Given demands should not be empty.");
            }
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
        
        public void OrderDemandsByUrgency()
        {
            // sort only, if there are more than one element
            if (_demands.Count > 1)
            {
                _demands.Sort((x, y) =>
                {
                    return x.GetDueTime().CompareTo(y.GetDueTime());
                });
            }
        }
        
        public HierarchyNumber GetHierarchyNumber()
        {
            return _hierarchyNumber;
        }

        public void Lock()
        {
            _isDemandsListLocked = true;
        }

        public int Size()
        {
            return _demands.Count;
        }

        public bool Any()
        {
            return _demands.Any();
        }

        public void Clear()
        {
            _demands.Clear();
        }

        public Quantity GetQuantityOfAll()
        {
            Quantity sumQuantity = new Quantity();
            foreach (var demand in _demands)
            {
                sumQuantity.IncrementBy(demand.GetQuantity());
            }

            return sumQuantity;
        }
    }
}