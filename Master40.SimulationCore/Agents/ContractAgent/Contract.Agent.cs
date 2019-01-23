using System;
using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;
using System.Linq;

namespace Master40.SimulationCore.Agents
{
    public partial class Contract : Agent
    {
        internal void TryFinialize() { Finish(); }
        /// <summary>
        /// Returns the (Dispo)Guardian for child creation
        /// Systemhirachie
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
        
        private Contract(ActorPaths actorPaths, long time, bool debug) 
            : base(actorPaths, time, debug, actorPaths.SystemAgent.Ref)
        {
            
        }

        protected override void OnChildAdd(IActorRef childRef)
        {
            var requestItem = Get<RequestItem>(Properties.REQUEST_ITEM);
            this.Send(Dispo.Instruction.RequestArticle.Create(requestItem, childRef));
            this.DebugMessage("Dispo<" + requestItem.Article.Name + "(OrderId: " + requestItem.OrderId + ") >");
        }

        protected override void Finish()
        {
            var r = this.Get<RequestItem>(Properties.REQUEST_ITEM);
            var childs = UntypedActor.Context.GetChildren();
            if (r.Provided && childs.Count() == 0)
            {
                base.Finish();
            }
        }


    }
}
