using System.Linq;
using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Agents.Guardian;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.ProductionAgent
{
    public partial class Production : Agent, IAgent
    {
        IActorRef IAgent.Guardian => this.ActorPaths.Guardians.Single(predicate: x => x.Key == GuardianType.Dispo).Value;

        // public Constructor
        public static Props Props(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Production(actorPaths, configuration, hiveConfig, time, debug, principal));
        }

        public Production(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, hiveConfig: hiveConfig, time: time, debug: debug, principal: principal)
        {
            DebugMessage(msg: "I'm Alive:" + Context.Self.Path);
            //this.Send(BasicInstruction.Initialize.Create(this.Context.Self, ProductionBehaviour.Get()));
        }
    }
}