using Akka.Actor;
using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static FSetupDefinitions;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class CapabilityDefinition
    {
        public M_ResourceCapability ResourceCapability { get; private set; } = new M_ResourceCapability();

        private List<FSetupDefinition> _setupDefinitions { get; } = new List<FSetupDefinition>();
        
        public CapabilityDefinition(M_ResourceCapability resourceCapabilities)
        {
            this.ResourceCapability = resourceCapabilities;
        }

        public bool HasCapability(M_ResourceCapability resourceCapability)
        {
            return ResourceCapability.Id == resourceCapability.Id;
        }

        public bool AddDefinition(FSetupDefinition setupDefinition)
        {
            this._setupDefinitions.Add(setupDefinition);
            return true;
        }
        public List<FSetupDefinition> GetAllSetupDefinitions => _setupDefinitions;

        internal FSetupDefinition GetSetupDefinitionBy(M_ResourceSetup setup)
        {
            var setupDefinition = _setupDefinitions.SingleOrDefault(x => x.SetupKey == setup.Id);
            if (setupDefinition == null)
            {
                setupDefinition = new FSetupDefinition(setup.Id, new List<IActorRef>());
                _setupDefinitions.Add(setupDefinition);
            }
            return setupDefinition;
        }
        internal FSetupDefinition GetSetupDefinitionBy(int setupId)
        {
            return _setupDefinitions.SingleOrDefault(x => x.SetupKey == setupId);
        }
    }
}
