using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;
using System.Linq;


namespace Master40.SimulationCore.Agents
{
    /// <summary>
    /// --------- General sequence
    /// 
    /// Contract -> Request Article     ->  Dispo
    ///                                     Dispo -> Request Stock for Article from -> Directory
    /// Directory -> Response with Stock -> Dispo
    ///                                     Dispo -> Request Article from Stock -> Stock
    /// Stock -> Response from stock    ->  Dispo                                    
    /// </summary>

    public partial class Dispo : Agent
    {
        internal IActorRef Guardian => this.ActorPaths.Guardians.Single(x => x.Key == GuardianType.Production).Value;

        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Dispo(actorPaths, time, debug, principal));
        }
        private Dispo(ActorPaths actorPaths, long time, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            System.Diagnostics.Debug.WriteLine("I'm Alive - DispoAgent");
            this.Send(BasicInstruction.Initialize.Create(this.Context.Self, DispoBehaviour.Get()));
        }

        protected override void OnInit(IBehaviour o)
        {
            this.Send(BasicInstruction.ChildRef.Create(this.Self, this.VirtualParent));
        }

        internal void ShutdownAgent()
        {
            this.Finish();
        }

        protected override void Finish()
        {
            var children = this.Context.GetChildren();
            if (this.Get<RequestItem>(Properties.REQUEST_ITEM).Provided == true && children.Count() == 0)
            {
                base.Finish();
            }
        }
    }
}
