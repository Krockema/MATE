using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ResourceDefinition
    {
        public string Name { get; private set; }

        public string Id { get; private set; }

        public IActorRef AgentRef { get; private set; }

        public ResourceDefinition(string name, string id, IActorRef actorRef)
        {
            Name = name;
            Id = id;
            AgentRef = actorRef;
        }

    }
}
