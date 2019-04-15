using System.Linq;
using Akka.Actor;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;

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
        internal IActorRef Guardian => this.ActorPaths.Guardians.Single(x => x.Key == GuardianType.Dispo).Value;
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
            var requestItem = Get<FRequestItem>(Properties.REQUEST_ITEM);
            this.Send(Dispo.Instruction.RequestArticle.Create(requestItem, childRef));
            this.DebugMessage("Dispo<" + requestItem.Article.Name + "(OrderId: " + requestItem.OrderId + ") >");
        }

        protected override void Finish()
        {
            
            var r = this.Get<FRequestItem>(Properties.REQUEST_ITEM);
            if (r.Provided && VirtualChilds.Count() == 0)
            {
                base.Finish();
            }
        }


    }
}
