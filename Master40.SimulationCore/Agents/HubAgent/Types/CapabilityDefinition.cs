using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class CapabilityDefinition
    {
        public M_ResourceCapability ResourceCapability { get; private set; } = new M_ResourceCapability();

        private List<SetupDefinition> _setupDefinitions { get; } = new List<SetupDefinition>();
        
        public CapabilityDefinition(M_ResourceCapability resourceCapabilities)
        {
            this.ResourceCapability = resourceCapabilities;
        }

        public bool HasCapability(M_ResourceCapability resourceCapability)
        {
            return ResourceCapability.Id == resourceCapability.Id;
        }

        public bool AddDefinition(SetupDefinition setupDefinition)
        {
            this._setupDefinitions.Add(setupDefinition);
            return true;
        }
        public List<SetupDefinition> GetAllSetupDefinitions => _setupDefinitions;

        internal SetupDefinition GetSetupDefinitionBy(M_ResourceSetup setup)
        {
            var setupDefinition = _setupDefinitions.SingleOrDefault(x => x.ResourceSetup.Id == setup.Id);
            if (setupDefinition == null)
            {
                setupDefinition = new SetupDefinition(setup);
                _setupDefinitions.Add(setupDefinition);
            }
            return setupDefinition;
        }
        internal SetupDefinition GetSetupDefinitionBy(int setupId)
        {
            return _setupDefinitions.SingleOrDefault(x => x.ResourceSetup.Id == setupId);
        }
    }
}
