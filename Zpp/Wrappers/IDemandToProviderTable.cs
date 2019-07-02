using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DemandToProviderDomain;

namespace Zpp
{
    public interface IDemandToProviderTable
    {
        void AddAll(IDemandToProviders demandToProviders);

        List<T_DemandToProvider> GetAll();

        IDemandToProviders ToDemandToProviders();
    }
}