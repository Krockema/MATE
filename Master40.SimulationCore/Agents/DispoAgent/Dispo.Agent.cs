using Akka.Actor;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Helper;
using System.Linq;

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

        protected override void OnChildAdd(IActorRef childRef)
        {
            var requestItem = ((Behaviour.Default)Behaviour).fArticle;
            this.Send(Production.Instruction.StartProduction.Create(requestItem, Sender));
            this.DebugMessage("Dispo<" + requestItem.Article.Name + "(OrderId: " + requestItem.CustomerOrderId + ") > ProductionStart has been send.");
        }
    }
}
