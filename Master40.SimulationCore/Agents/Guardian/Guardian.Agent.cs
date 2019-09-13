using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Helper;

namespace Master40.SimulationCore.Agents.Guardian
{
    /// <summary>
    /// Guardian Action is an Supervising Actor to Create Child Actors on Command and Controll their LifeCycle
    /// </summary>
    public partial class Guardian : Agent
    {
        /// <summary>
        /// Basic Agent
        /// </summary>
        /// <param name="actorPaths"></param>
        /// <param name="time">Current time span</param>
        /// <param name="debug">Parameter to activate Debug Messages on Agent level</param>


        public Guardian(ActorPaths actorPaths, long time, bool debug)
            : base(actorPaths: actorPaths, time: time, debug: false, principal: null)
        {
            DebugMessage(msg: "I'm alive: " + Self.Path.ToStringWithAddress());
        }

        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(factory: () => new Guardian(actorPaths, time, debug));
        }

        public override void AroundPostStop()
        {
            // Only for debugging reasons, make sure that all childs finished if all orders are delivered
            System.Diagnostics.Debug.WriteLine($"{this.Self.Path.Name} children counter: {((GuardianBehaviour)this.Behaviour).counterChilds}");
            base.AroundPostStop();
        }


        protected override void Finish()
        {
            
            ((GuardianBehaviour)this.Behaviour).counterChilds--;
            //System.Diagnostics.Debug.WriteLine($"{Sender.Path.Name} finished child and has {((GuardianBehaviour)this.Behaviour).counterChilds} now");

        }
    }
}
