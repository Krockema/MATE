using Akka.Actor;
using Mate.DataCore.Nominal.Model;

namespace Mate.Production.Core.Environment.Records
{
    public record RequestResourceRecord
    (
        string Discriminator,
        AgentType  ResourceType,
        IActorRef actorRef
    );
}
