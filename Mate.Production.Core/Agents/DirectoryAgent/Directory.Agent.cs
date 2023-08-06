using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.DirectoryAgent
{
    public partial class Directory : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug)
        {
            return Akka.Actor.Props.Create(factory: () => new Directory(actorPaths, configuration, hiveConfig, time, debug));
        }

        public Directory(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug) 
            : base(actorPaths: actorPaths, configuration: configuration, hiveConfig: hiveConfig, time: time, debug: debug, principal: ActorRefs.Nobody)
        {

        }
    }
}
