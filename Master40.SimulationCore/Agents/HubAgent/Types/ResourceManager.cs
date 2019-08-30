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
        private List<ResourceSetup> _resources = new List<ResourceSetup>();
        
        public ResourceManager()
        {
            
        }

        /// <summary>
        /// TODO make bool?
        /// </summary>
        /// <param name="resourceSetups"></param>
        public void Add(ResourceSetup resourceSetups)
        {
            _resources.Add(resourceSetups);
        }

        public List<IActorRef> GetResourceByTool(M_ResourceTool resourceTool)
        {
            var resourceAgents = new List<IActorRef>();
            foreach (var resource in _resources)
            {
                if (resource.HasTool(resourceTool)) { 
                    resourceAgents.Add(resource.GetActorRef());
                }
            }
            return resourceAgents;
        }

    }
}
