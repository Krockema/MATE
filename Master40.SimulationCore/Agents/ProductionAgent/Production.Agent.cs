using Akka.Actor;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Helper;
using System.Linq;

namespace Master40.SimulationCore.Agents.ProductionAgent
{
    public partial class Production : Agent
    {
        internal IActorRef Guardian => this.ActorPaths.Guardians.Single(x => x.Key == GuardianType.Dispo).Value;

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
            var requestItem = ((Behaviour.Default)Behaviour).childOperations.Dequeue();
            this.Send(Dispo.Instruction.RequestArticle.Create(requestItem, childRef));
            this.DebugMessage("Production<" + requestItem.Article.Name + "(OrderId: " + requestItem.CustomerOrderId + ") >");
        }

        protected override void Finish()
        {

        }
    }
}