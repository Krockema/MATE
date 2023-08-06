using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.ResourceAgent
{
    public partial class Resource : Agent
    {
        internal M_Resource _resource;
        // public Constructor
        public static Props Props(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, M_Resource resource, Time time, bool debug, IActorRef principal, IActorRef measurementActorRef)
        {
            return Akka.Actor.Props.Create(factory: () => new Resource(actorPaths, configuration, hiveConfig, resource, time, debug, principal, measurementActorRef));
        }

        public static Props Props(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Resource(actorPaths, configuration, hiveConfig, time, debug, principal));
        }

        public Resource(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, M_Resource resource, Time time, bool debug, IActorRef principal, IActorRef measurementActorRef) 
            : base(actorPaths: actorPaths, configuration: configuration, hiveConfig: hiveConfig, time: time, debug: debug, principal: principal)
        {
            _resource = resource;
        }

        public Resource(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, hiveConfig: hiveConfig, time: time, debug: debug, principal: principal)
        {

        }

        protected override void PostAdvance()
        {
            this.Behaviour.PostAdvance();
        }
    }
}