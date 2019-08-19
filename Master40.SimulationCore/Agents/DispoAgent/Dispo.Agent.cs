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

    public partial class Dispo : Agent, IAgent
    {

        IActorRef IAgent.Guardian => this.ActorPaths.Guardians.Single(predicate: x => x.Key == GuardianType.Production).Value;

        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Dispo(actorPaths, time, debug, principal));
        }
        public Dispo(ActorPaths actorPaths, long time, bool debug, IActorRef principal) : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            DebugMessage(msg: "I'm Alive: " + Context.Self.Path);
            //this.Do(BasicInstruction.Initialize.Create(Self, DispoBehaviour.Get()));
        }

        protected override void OnChildAdd(IActorRef childRef)
        {
            var fArticle = ((Behaviour.Default)Behaviour)._fArticle;
            this.Send(instruction: Production.Instruction.StartProduction.Create(message: fArticle, target: Sender));
            this.DebugMessage(msg: "Dispo<" + fArticle.Article.Name + "(OrderId: " + fArticle.CustomerOrderId + ") > ProductionStart has been sent.");
        }
    }
}
