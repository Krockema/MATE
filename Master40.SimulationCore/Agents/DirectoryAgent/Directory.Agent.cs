using Akka.Actor;
using Master40.SimulationCore.Helper;

namespace Master40.SimulationCore.Agents.DirectoryAgent
{
    public partial class Directory : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(factory: () => new Directory(actorPaths, time, debug));
        }

        public Directory(ActorPaths actorPaths, long time, bool debug) : base(actorPaths: actorPaths, time: time, debug: debug, principal: ActorRefs.Nobody)
        {

        }
        protected override void OnChildAdd(IActorRef childRef)
        {

        }
    }
}
