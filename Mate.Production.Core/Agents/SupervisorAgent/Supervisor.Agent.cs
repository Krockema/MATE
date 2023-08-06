using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.SupervisorAgent
{
    public partial class Supervisor : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths
                                    , IHiveConfig hiveConfig
                                    , Configuration configuration
                                    , Time time
                                    , bool debug
                                    , IActorRef  principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Supervisor(actorPaths, hiveConfig, configuration,  time, debug, principal));
        }

        public Supervisor(ActorPaths actorPaths
                            , IHiveConfig hiveConfig
                            , Configuration configuration
                            , Time time
                            , bool debug
                            , IActorRef principal) 
            : base(actorPaths: actorPaths, hiveConfig: hiveConfig, configuration: configuration, time: time, debug: debug, principal: principal)
        {
        }

        protected override void Finish()
        {
            if (Sender == ActorPaths.SimulationContext.Ref)
            {
                base.Finish();
            }
        }

    }
}
