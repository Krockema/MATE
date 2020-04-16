using Master40.DB.DataModel;
using System;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class CapabilityInUse
    {
        public M_ResourceCapabilityProvider ResourceCapabilityProvider { get; private set; }
        private bool SetupPhase { get; set; }
        public bool IsSetupPhase => SetupPhase;

        public CapabilityInUse()
        {
            ResourceCapabilityProvider = null;
            SetupPhase = false;
        }
        /// <summary>
        /// Start the SetupPhase with the tool and make a flag for currently in setupPhase
        /// </summary>
        /// <param name="resourceTool"></param>
        /// <returns></returns>
        public bool Mount(M_ResourceCapabilityProvider resourceCapabilityProvider)
        {
            if (SetupPhase != false) return false;
            ResourceCapabilityProvider = resourceCapabilityProvider;
            return true;
        }

        public bool IsSet(M_ResourceCapability resourceCapability)
        {
            if (ResourceCapabilityProvider == null || ResourceCapabilityProvider.Id != resourceCapability.Id ) return false;
            return true;
        }

        public int SetupId()
        {
            if (ResourceCapabilityProvider == null) return -1;
            return ResourceCapabilityProvider.Id;
        }

        /// <summary>
        ///  TODO can be enhanced to make reset times possible, i.e. cooling down phases after working with tool
        /// </summary>
        /// <returns></returns>
        public bool Dismount()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finish the setup of the resource and resource can now be used for working
        /// </summary>
        public void FinishSetup()
        {
            SetupPhase = false;
        }
    }
}
