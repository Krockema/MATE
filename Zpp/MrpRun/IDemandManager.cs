using System.Collections.Generic;
using Master40.DB.Interfaces;
using Zpp.Wrappers;
using Zpp.WrappersForPrimitives;

namespace Zpp
{
    public interface DemandManager
    {

        void AddDemand(Demand demand);
        
        void AddDemands(List<Demand> demands);

        List<Demand> GetDemands();

        void OrderDemandsByUrgency();

        List<IProvider> GetProvidersOfDemand(int demandId);

        void AddProviderForDemand(int demandId, int providerId);

        HierarchyNumber GetHierarchyNumber();

        /// <summary>
        /// demandsList are not allowed to be expanded after this call
        /// </summary>
        void LockDemandsList();

        void PersistDemands();
    }
}