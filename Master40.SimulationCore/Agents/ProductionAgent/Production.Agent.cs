using Akka.Actor;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Helper;
using System.Linq;

namespace Master40.SimulationCore.Agents.ProductionAgent
{
    public partial class Production : Agent
    {
        internal new IActorRef Guardian => this.ActorPaths.Guardians.Single(x => x.Key == GuardianType.Dispo).Value;

        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Production(actorPaths, time, debug, principal));
        }

        public Production(ActorPaths actorPaths, long time, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            DebugMessage("I'm Alive:" + Context.Self.Path);
            //this.Send(BasicInstruction.Initialize.Create(this.Context.Self, ProductionBehaviour.Get()));
        }
        protected override void OnChildAdd(IActorRef childRef)
        {
            var fArticle = ((Behaviour.Default)Behaviour)._childArticles.Dequeue();
            this.Send(Dispo.Instruction.RequestArticle.Create(fArticle, childRef));
            this.DebugMessage(
                $"Create Dispo Agent for {fArticle.Article.Name} (Key: {fArticle.Key}, OrderId: {fArticle.CustomerOrderId})");
        }

    }
}