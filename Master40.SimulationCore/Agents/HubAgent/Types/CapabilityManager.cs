using Akka.Actor;
using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class CapabilityManager
    {
        private List<CapabilityDefinition> _capabilityDefinitions = new List<CapabilityDefinition>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceSetups"></param>
        public void Add(CapabilityDefinition capabilityDefinition)
        {
            _capabilityDefinitions.Add(capabilityDefinition);
        }

        public CapabilityDefinition GetResourcesByCapability(M_ResourceCapability resourceCapability)
        {
            return _capabilityDefinitions.Single(x => x.HasCapability(resourceCapability));
        }

        internal CapabilityDefinition GetCapabilityDefinition(M_ResourceCapability capability)
        {
            var capabilityDefinition =
                _capabilityDefinitions.SingleOrDefault(x => x.ResourceCapability.Id == capability.Id);
            if (capabilityDefinition == null)
            {
                capabilityDefinition = new CapabilityDefinition(capability);
                _capabilityDefinitions.Add(capabilityDefinition);
            }
            return capabilityDefinition;
        }

        public List<SetupDefinition> GetAllSetupDefintions(M_ResourceCapability capability)
        {
            return _capabilityDefinitions.Single(x => x.ResourceCapability.Id == capability.Id).GetAllSetupDefinitions;

        }
        

    }
}
