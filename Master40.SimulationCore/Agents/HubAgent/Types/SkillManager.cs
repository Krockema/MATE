using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class SetupManager
    {
        private List<M_ResourceSetup> _resourceSetups { get; } = new List<M_ResourceSetup>();

        public SetupManager(List<M_ResourceSetup> resourceSetups)
        {
            this._resourceSetups = resourceSetups;
        }

        public List<IActorRef> GetResourceByTool(M_ResourceTool resourceTool)
        {
            var resourceAgents = new List<IActorRef>();
            return resourceAgents;
        }

    }
}
