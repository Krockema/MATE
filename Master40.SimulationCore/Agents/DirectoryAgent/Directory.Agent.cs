using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Environment;

namespace Master40.SimulationCore.Agents.DirectoryAgent
{
    public partial class Directory : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, Configuration configuration, long time, bool debug)
        {
            return Akka.Actor.Props.Create(factory: () => new Directory(actorPaths, configuration, time, debug));
        }

        public Directory(ActorPaths actorPaths, Configuration configuration, long time, bool debug) 
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: ActorRefs.Nobody)
        {

        }
    }
}
