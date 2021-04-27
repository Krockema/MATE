using System.Linq;
using Akka.Actor;
using Mate.Production.Core.Agents.ContractAgent.Behaviour;
using Mate.Production.Core.Agents.Guardian;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.ContractAgent
{
    public partial class Contract : Agent, IAgent
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
        IActorRef IAgent.Guardian => this.ActorPaths.Guardians.Single(predicate: x => x.Key == GuardianType.Dispo).Value;

        // public Constructor
        public static Props Props(ActorPaths actorPaths, Configuration configuration, long time, bool debug)
        {
            return Akka.Actor.Props.Create(factory: () => new Contract(actorPaths, configuration, time, debug));   
        }
        
        public Contract(ActorPaths actorPaths, Configuration configuration, long time, bool debug) 
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: actorPaths.SystemAgent.Ref)
        {
            DebugMessage(msg: "I'm Alive:" + Context.Self.Path);
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
