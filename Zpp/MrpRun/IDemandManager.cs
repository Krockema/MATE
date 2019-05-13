using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp
{
    public interface IDemandManager
    {
        IDemand GetDemandById(int id);

        void AddDemand(IDemand demand);

        List<IDemand> GetDemands();

        void orderDemandsByUrgency();

        List<IProvider> getProvidersOfDemand(int demandId);
    }
}