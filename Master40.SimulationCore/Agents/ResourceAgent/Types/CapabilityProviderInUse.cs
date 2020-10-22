using Master40.DB.DataModel;
using System;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class CapabilityProviderInUse
    {
        public M_ResourceCapabilityProvider ResourceCapabilityProvider { get; private set; }
        private bool SetupPhase { get; set; }
        public CapabilityProviderInUse()
        {
            ResourceCapabilityProvider = null;
        }
        /// <summary>
        /// Start the SetupPhase with the tool and make a flag for currently in setupPhase
        /// </summary>
        /// <param name="resourceTool"></param>
        /// <returns></returns>
        public bool Mount(M_ResourceCapabilityProvider resourceCapabilityProvider)
        {
            ResourceCapabilityProvider = resourceCapabilityProvider;
            return true;
        }
    }
}
