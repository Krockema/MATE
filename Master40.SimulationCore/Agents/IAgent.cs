using Akka.Actor;

namespace Master40.SimulationCore.Agents
{
    public  interface IAgent
    {
        IActorRef Guardian { get; }
    }
}
