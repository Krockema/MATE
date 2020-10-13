using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Master40.DB.Nominal.Model;
using Master40.SimulationCore.Helper;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ResourceDefinition
    {
        public string Name { get; private set; }

        public int Id { get; private set; }

        public IActorRef AgentRef { get; private set; }

        public string GroupId { get; private set; }

        public ResourceType ResourceType { get; private set; }

        public ResourceDefinition(string name, int id, IActorRef actorRef, string groupId, ResourceType resourceType)
        {
            Name = name.ToActorName();
            Id = id;
            AgentRef = actorRef;
            GroupId = groupId;
            ResourceType = resourceType;
        }

    }
}
