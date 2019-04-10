using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.DispoAgent
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
        public Dispo(ActorPaths actorPaths, long time, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            DebugMessage("I'm Alive: " + Context.Self.Path);
            //this.Do(BasicInstruction.Initialize.Create(Self, DispoBehaviour.Get()));
        }

        protected override void OnInit(IBehaviour o)
        {

        }

        protected override void OnChildAdd(IActorRef childRef)
        {
            var requestItem = Get<FRequestItem>(Properties.REQUEST_ITEM);
            this.Send(Production.Instruction.StartProduction.Create(requestItem, Sender));
            this.DebugMessage("Dispo<" + requestItem.Article.Name + "(OrderId: " + requestItem.OrderId + ") > ProductionStart has been send.");
        }

        internal void ShutdownAgent()
        {
            this.Finish();
        }

        protected override void Finish()
        {
            var children = this.Context.GetChildren();
            if (this.Get<FRequestItem>(Properties.REQUEST_ITEM).Provided == true && children.Count() == 0)
            {
                base.Finish();
            }
        }
    }
}
