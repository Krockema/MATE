using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ToolCapabilityPair
    {
        public M_ResourceTool _resourceTool { get; private set; }
        public M_ResourceCapability _resourceCapability { get; private set; }

        public ToolCapabilityPair(M_ResourceTool resourceTool, M_ResourceCapability resourceCapability)
        {
            this._resourceTool = resourceTool;
            this._resourceCapability = resourceCapability;
        }
    }
}
