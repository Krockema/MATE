using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent.Types;

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
            return Akka.Actor.Props.Create(factory: () => new Hub(actorPaths, time, simtype, debug, principal));
        }

        public Hub(ActorPaths actorPaths, long time, SimulationType simtype, bool debug, IActorRef principal) : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            DebugMessage(msg: "I'm Alive:" + Context.Self.Path);
            this.Do(o: BasicInstruction.Initialize.Create(target: Self, message: HubAgent.Behaviour.Factory.Get(simType: simtype)));
        }
    }
}