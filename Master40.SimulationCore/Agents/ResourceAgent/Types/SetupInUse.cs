using Master40.DB.DataModel;
using System;
using System.Resources;
using static FSetupDefinitions;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class SetupInUse
    {
        public M_ResourceSetup ResourceSetup { get; private set; }
        private FSetupDefinition currentSetupDefinition { get; set; }
        private bool SetupPhase { get; set; }
        public bool IsSetupPhase => SetupPhase;

        public SetupInUse()
        {
            ResourceSetup = null;
            SetupPhase = false;
        }
        /// <summary>
        /// Start the SetupPhase with the tool and make a flag for currently in setupPhase
        /// </summary>
        /// <param name="resourceTool"></param>
        /// <returns></returns>
        public bool Mount(M_ResourceSetup resourceSetup)
        {
            if (SetupPhase != false) return false;
            ResourceSetup = resourceSetup;
            return true;
        }

        public bool IsSet(M_ResourceCapability resourceCapability)
        {
            if (ResourceSetup == null || ResourceSetup.Id != resourceCapability.Id ) return false;
            return true;
        }

        public int SetupId()
        {
            if (ResourceSetup == null) return -1;
            return ResourceSetup.Id;
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
