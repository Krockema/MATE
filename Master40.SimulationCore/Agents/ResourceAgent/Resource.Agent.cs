using Akka.Actor;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.ResourceAgent
{
    public partial class Resource : Agent
    {
        internal IActorRef MeasurementActorRef;

        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug, IActorRef principal, IActorRef measurementActorRef = null)
        {
            return Akka.Actor.Props.Create(factory: () => new Resource(actorPaths, time, debug, principal, measurementActorRef));    
        }

        public Resource(ActorPaths actorPaths, long time, bool debug, IActorRef principal, IActorRef measurementActorRef) : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            MeasurementActorRef = measurementActorRef;
        }

        protected override void OnInit(IBehaviour o)
        {
            Behaviour.AfterInit();
        }
    }
}