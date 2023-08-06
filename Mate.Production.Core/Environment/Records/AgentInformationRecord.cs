using Akka.Actor;

namespace Mate.Production.Core.Environment.Records
{
    public record AgentInformationRecord(AgentType FromType
                                        , string RequiredFor
                                        , IActorRef Ref)
    {
        public AgentInformationRecord UpdateRef(IActorRef r) => this with { Ref = r };
    }
}