using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public interface ICapabilityManager
    {
        List<ResourcesRequired> GetRequiredResourcesFor(M_ResourceCapability capability);
        bool AddSetup(M_ResourceSetup setup);
        bool Replace(M_ResourceSetup setup);
        bool RemoveSetup(M_ResourceSetup setup);

    }
}