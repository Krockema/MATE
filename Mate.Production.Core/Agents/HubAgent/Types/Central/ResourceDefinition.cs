using Akka.Actor;
using Mate.DataCore.Nominal.Model;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.HubAgent.Types.Central
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
