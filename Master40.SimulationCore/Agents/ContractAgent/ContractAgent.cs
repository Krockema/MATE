using System;
using Akka.Actor;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;
using Master40.SimulationCore.MessageTypes;
using System.Linq;

namespace Master40.SimulationCore.Agents
{
    public partial class ContractAgent : Agent
    {

        internal void TryFinialize() { Finish(); }
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new ContractAgent(actorPaths, time, debug));   
        }
        // 
        public ContractAgent(ActorPaths actorPaths, long time, bool debug) 
            : base(actorPaths, time, debug)
        {
        }

        protected override void Finish()
        {
            var r = this.Get<RequestItem>(RequestItem);
            var childs = UntypedActor.Context.GetChildren();
            if (r.Provided && childs.Count() == 0)
            {
                base.Finish();
            }
        }


    }
}
