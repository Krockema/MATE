using Akka.Actor;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.SupervisorAgent
{
    public partial class Supervisor : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths
                                    ,Configuration configuration
                                    , long time
                                    , bool debug
                                    , IActorRef  principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Supervisor(actorPaths, configuration,  time, debug, principal));
        }

        public Supervisor(ActorPaths actorPaths
                            ,Configuration configuration
                            , long time
                            , bool debug
                            , IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: principal)
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
