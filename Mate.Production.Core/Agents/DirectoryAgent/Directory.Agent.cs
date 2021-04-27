using Akka.Actor;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.DirectoryAgent
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
