using Akka.Actor;
using Akka.Event;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Agents;
using Master40.SimulationImmutables;
using System;
using System.Linq;


namespace Master40.SimulationCore.Agents
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

    public partial class DispoAgent : Agent
    {
      

        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new DispoAgent(actorPaths, time, debug));
        }
        public DispoAgent(ActorPaths actorPaths, long time, bool debug) : base(actorPaths, time, debug)
        {
            System.Diagnostics.Debug.WriteLine("I'm Alive - DispoAgent");
        }

        protected void DoNot(object o)
        {
            switch (o)
            {
                // case BasicInstruction.ResponseFromHub rfd: ResponseFromHub(rfd.GetObjectFromMessage); break;
                // case Instruction.ResponseFromSystemForBom a: ResponseFromSystemForBom(a.GetObjectFromMessage); break;
                // case Instruction.RequestProvided rp: RequestProvided(rp.GetObjectFromMessage); break;
                // case Instruction.WithdrawMaterialsFromStock wm: WithdarwMaterial(); break;
                default: throw new Exception("Invalid Message Object.");
            }
        }

        internal void ShutdownAgent()
        {
            this.Finish();
        }

        protected override void Finish()
        {
            var children = this.Context.GetChildren();
            if (this.Get<RequestItem>(RequestItem).Provided == true && children.Count() == 0)
            {
                base.Finish();
            }
        }
    }
}
