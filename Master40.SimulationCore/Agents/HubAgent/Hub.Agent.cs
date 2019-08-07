using Akka.Actor;
using Master40.SimulationCore.Helper;

namespace Master40.SimulationCore.Agents.HubAgent
{
    /// <summary>
    /// Alternative Namen; ResourceAllocation, RessourceGroup, Mediator, Coordinator, Hub
    /// </summary>
    public partial class Hub : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, string skillGroup, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Hub(actorPaths, time, skillGroup, debug, principal));
        }

        public Hub(ActorPaths actorPaths, long time, string skillGroup, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            DebugMessage("I'm Alive:" + Context.Self.Path);
            this.Do(BasicInstruction.Initialize.Create(Self, HubBehaviour.Get(skillGroup)));
        }
    }
}