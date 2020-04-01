using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ResourceCapabilityPair
    {
        public M_Resource _resourceTool { get; private set; }
        public M_ResourceCapability _resourceCapability { get; private set; }

        public ResourceCapabilityPair(M_Resource resource, M_ResourceCapability resourceCapability)
        {
            this._resourceTool = resource;
            this._resourceCapability = resourceCapability;
        }
    }
}
