using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ResourceManager
    {
        private List<SetupManager> _resources = new List<SetupManager>();

        private List<ToolCapabilityPair> _toolCapabilityPairs = new List<ToolCapabilityPair>();
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceSetups"></param>
        public void Add(SetupManager setupManager)
        {
            _resources.Add(setupManager);
            AddToolCapacitiyPairs(setupManager);
        }

        private void AddToolCapacitiyPairs(SetupManager setupManager)
        {
            foreach (var setup in setupManager._resourceSetups)
            {
                var toolCapabilityPair = new ToolCapabilityPair(setup.ChildResource, setup.ResourceCapability);
                if (!_toolCapabilityPairs.Exists(x => x._resourceCapability.Name.Equals(toolCapabilityPair._resourceCapability.Name) && x._resourceTool.Name.Equals(toolCapabilityPair._resourceTool.Name)))
                {
                    _toolCapabilityPairs.Add(toolCapabilityPair);
                }
            }
        }

        public List<IActorRef> GetResourceByTool(M_ResourceCapability resourceTool)
        {
            var resourceAgents = new List<IActorRef>();
            foreach (var resource in _resources)
            {
                if (resource.HasTool(resourceTool: resourceTool)) { 
                    resourceAgents.Add(resource.GetActorRef());
                }
            }
            return resourceAgents;
        }

        public ToolCapabilityPair GetToolCapabilityPair(M_ResourceCapability capability)
        { 
            return _toolCapabilityPairs.Single(x => x._resourceTool.Name.Equals(capability.Name));
        }

    }
}
