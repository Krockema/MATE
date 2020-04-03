using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ContractAgent;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class SetupInUse
    {
        public M_Resource Resource { get; private set; }
        private bool SetupPhase { get; set; }

        public bool IsSetupPhase => SetupPhase;

        public SetupInUse()
        {
            Resource = null;
            SetupPhase = false;
        }
        /// <summary>
        /// Start the SetupPhase with the tool and make a flag for currently in setupPhase
        /// </summary>
        /// <param name="resourceTool"></param>
        /// <returns></returns>
        public bool Mount(M_Resource resource)
        {
            if (SetupPhase != false) return false;
            Resource = resource;
            return true;
        }

        public bool IsSet(M_ResourceCapability resourceTool)
        {
            if (Resource == null || Resource.Id != resourceTool.Id ) return false;
            return true;
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
