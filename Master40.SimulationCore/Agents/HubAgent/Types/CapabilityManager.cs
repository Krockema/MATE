using Master40.DB.DataModel;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class CapabilityManager : ICapabilityManager
    {
        private M_ResourceSetup SetupHierarchy { get; set; }

        private CapabilityManager()
        {

        }

        public List<ResourcesRequired> GetRequiredResourcesFor(M_ResourceCapability capability)
        {
            throw new System.NotImplementedException();
        }

        public bool AddSetup(M_ResourceSetup setup)
        {
            throw new System.NotImplementedException();
        }

        public bool Replace(M_ResourceSetup setup)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveSetup(M_ResourceSetup setup)
        {
            throw new System.NotImplementedException();
        }
    }
}