using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.DB.Enums;

namespace Master40.SimulationCore.Agents.HubAgent
{
    /// <summary>
    /// Alternative Namen; ResourceAllocation, RessourceGroup, Mediator, Coordinator, Hub
    /// </summary>
    public partial class Hub : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, SimulationType simtype, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Hub(actorPaths, time, simtype, debug, principal));
        }

        public Hub(ActorPaths actorPaths, long time, SimulationType simtype, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            DebugMessage("I'm Alive:" + Context.Self.Path);
            this.Do(BasicInstruction.Initialize.Create(Self, HubAgent.Behaviour.Factory.Get(simtype )));
        }
    }
}