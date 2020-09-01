using Akka.Actor;
using Master40.SimulationCore.Agents.SupervisorAgent.Behaviour;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.SupervisorAgent
{
    public partial class Supervisor : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths
                                        , long time
                                        , bool debug
                                        , IActorRef  principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Supervisor(actorPaths, time, debug, principal));
        }

        public Supervisor(ActorPaths actorPaths
                                        , long time
                                        , bool debug
                                        , IActorRef principal
                                        ) 
            : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
        }

        protected override void OnInit(IBehaviour o)
        {
            ((Default) Behaviour).AfterInit();
        }

        /// <summary>
        /// After a child has been ordered from Guardian a ChildRef will be returned by the responsible child
        /// it has been allready added to this.VirtualChilds at this Point
        /// </summary>
        /// <param name="childRef"></param>
        protected override void OnChildAdd(IActorRef childRef)
        {
            ((Behaviour.Default)Behaviour).OnChildAdd(childRef);
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
