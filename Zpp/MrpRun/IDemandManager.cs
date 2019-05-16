using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp
{
    public interface IDemandManager
    {
        IDemand GetDemandById(int id);

        void AddDemand(IDemand demand);

        List<IDemand> GetDemands();

        void OrderDemandsByUrgency();

        List<IProvider> GetProvidersOfDemand(int demandId);

        void AddProviderForDemand(int demandId, int providerId);

        int GetHierarchyNumber();

        /// <summary>
        /// demandsList are not allowed to be expanded after this call
        /// </summary>
        void LockDemandsList();

        void PersistDemands();
    }
}