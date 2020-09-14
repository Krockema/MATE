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

        public string GroupId { get; private set; }

        public int ResourceType { get; private set; }

        public ResourceDefinition(string name, string id, IActorRef actorRef, string groupId, int resourceType)
        {
            Name = name;
            Id = id;
            AgentRef = actorRef;
            GroupId = groupId;
            ResourceType = resourceType;
        }

    }
}
