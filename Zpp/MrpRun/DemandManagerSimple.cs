using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp
{
    /// <summary>
    /// An trivial implementation of IDemandManager, which is for performance reasons not well suited
    /// </summary>
    public class DemandManagerSimple : IDemandManager
    {
        private readonly List<IDemand> DEMANDS = new List<IDemand>();
        private readonly Dictionary<int, List<int>> demandsHavingProviders = new Dictionary<int, List<int>>();
        
        public DemandManagerSimple()
        {
            
        }
        
        public IDemand GetDemandById(int id)
        {
            foreach (IDemand demand in DEMANDS)
            {
                if (demand.Id.Equals(id))
                {
                    return demand;
                }
            }

            return null;
        }

        public void AddDemand(IDemand demand)
        {
            DEMANDS.Add(demand);
        }

        public List<IDemand> GetDemands()
        {
            return DEMANDS;
        }
        
        public void orderDemandsByUrgency()
        {
            DEMANDS.Sort((x, y) => x.GetDueTime().CompareTo(y.GetDueTime()));
        }

        List<IProvider> getProvidersOfDemand(int demandId)
        {
            
        }
        
        public List<IDemand> ToIDemands(List<T_Demand> t_demands)
        {
            return t_demands.Select(x => x.ToIDemand(_productionDomainContext, x)).ToList();
        }
    }
}