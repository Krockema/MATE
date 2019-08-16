using Akka.Actor;
using Master40.SimulationCore.Agents.ContractAgent.Behaviour;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Helper;
using System.Linq;

namespace Master40.SimulationCore.Agents.ContractAgent
{
    public partial class Contract : Agent
    {
        internal void TryFinialize() { Finish(); }
        /// <summary>
        /// Returns the (Dispo)Guardian for child creation
        /// Systemhirachie:
        /// Supervisor
        /// '->Contract
        /// '--->Dispo
        /// '----->Production
        /// </summary>
        internal new IActorRef Guardian => this.ActorPaths.Guardians.Single(x => x.Key == GuardianType.Dispo).Value;
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new Contract(actorPaths, time, debug));   
        }
        
        public Contract(ActorPaths actorPaths, long time, bool debug) 
            : base(actorPaths, time, debug, actorPaths.SystemAgent.Ref)
        {
            DebugMessage("I'm Alive:" + Context.Self.Path);
        }

        protected override void OnChildAdd(IActorRef childRef)
        {
            var fArticle = ((IDefaultProperties)Behaviour)._fArticle;
            this.Send(Dispo.Instruction.RequestArticle.Create(fArticle, childRef));
            this.DebugMessage("Dispo<" + fArticle.Article.Name + "(OrderId: " + fArticle.CustomerOrderId + ")>");
        }

        protected override void Finish()
        {
            var fArticle = ((IDefaultProperties)Behaviour)._fArticle;
            if (fArticle.IsProvided && VirtualChildren.Count() == 0)
            {
                base.Finish();
            }
        }


    }
}
