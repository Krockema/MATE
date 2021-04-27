using System.Linq;
using Akka.Actor;
using Mate.Production.Core.Agents.Guardian;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.ProductionAgent
{
    public partial class Production : Agent, IAgent
    {
        IActorRef IAgent.Guardian => this.ActorPaths.Guardians.Single(predicate: x => x.Key == GuardianType.Dispo).Value;

        // public Constructor
        public static Props Props(ActorPaths actorPaths, Configuration configuration,long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Production(actorPaths, configuration, time, debug, principal));
        }

        public Production(ActorPaths actorPaths, Configuration configuration, long time, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: principal)
        {
            DebugMessage(msg: "I'm Alive:" + Context.Self.Path);
            //this.Send(BasicInstruction.Initialize.Create(this.Context.Self, ProductionBehaviour.Get()));
        }
    }
}