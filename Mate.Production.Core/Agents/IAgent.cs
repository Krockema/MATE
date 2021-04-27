using Akka.Actor;

namespace Mate.Production.Core.Agents
{
    public  interface IAgent
    {
        IActorRef Guardian { get; }
    }
}
