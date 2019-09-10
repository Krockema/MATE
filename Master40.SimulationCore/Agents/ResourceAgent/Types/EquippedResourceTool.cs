using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ContractAgent;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class EquippedResourceTool
    {
        public M_ResourceTool ResourceTool { get; private set; }
        private bool SetupPhase { get; set; }

        public bool IsSetupPhase => SetupPhase;

        public EquippedResourceTool()
        {
            ResourceTool = null;
            SetupPhase = false;
        }
        /// <summary>
        /// Start the SetupPhase with the tool and make a flag for currently in setupPhase
        /// </summary>
        /// <param name="resourceTool"></param>
        /// <returns></returns>
        public bool Mount(M_ResourceTool resourceTool)
        {
            if (SetupPhase != false) return false;
            SetupPhase = true;
            ResourceTool = resourceTool;
            return true;
        }

        public bool IsSet(M_ResourceTool resourceTool)
        {
            if (ResourceTool == null || ResourceTool.Id != resourceTool.Id ) return false;
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
