using Akka.Actor;
using Master40.SimulationCore.Helper;

namespace Master40.SimulationCore.Agents.StorageAgent
{
    public partial class Storage : Agent
    {

        // Statistic 
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Storage(actorPaths, time, debug, principal));
        }

        public Storage(ActorPaths actorPaths, long time, bool debug, IActorRef principal) : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            
        }
    }
}
