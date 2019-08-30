using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ResourceSetup
    {
        private List<M_ResourceSetup> _resourceSetups { get; } = new List<M_ResourceSetup>();

        private IActorRef _resourceAgent { get; set; }

        private string _resourceName { get; set; } 

        public ResourceSetup(List<M_ResourceSetup> resourceSetups, IActorRef resourceAgent, string resourceName)
        {
            this._resourceSetups = resourceSetups;
            this._resourceAgent = resourceAgent;
            this._resourceName = resourceName;
        }

        public bool HasTool(M_ResourceTool resourceTool)
        {
            var value = _resourceSetups.Any(x => x.ResourceTool.Id == resourceTool.Id);
            return value;
        }

        public IActorRef GetActorRef()
        {
            return _resourceAgent;
        }

    }
}
