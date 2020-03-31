using Master40.DB.DataModel;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class CapabilityManager : ICapabilityManager
    {
        public List<M_ResourceSetup> SetupHierarchy { get; private set; }
        
        public CapabilityManager()
        {

        }

        //theoretisch müssen wir eine Liste in einer Liste zurückgeben
        public List<ResourcesRequired> GetRequiredResourcesFor(M_ResourceCapability capability)
        {
            // Get a concrete capability like saw 10mm
            foreach (M_ResourceSetup setup in SetupHierarchy)
            {
                
            }
            // return all subtrees of the 1st level resources


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