using Akka.Actor;

namespace Mate.Production.Core.Environment.Records.Central
{
    public record CentralResourceRegistrationRecord
    (
        int ResourceId,
        string ResourceName,
        IActorRef ResourceActorRef,
        string ResourceGroupId,
        int ResourceType
    );
}

