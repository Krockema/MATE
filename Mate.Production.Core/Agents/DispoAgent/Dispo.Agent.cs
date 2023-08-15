using System.Linq;
using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Agents.Guardian;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.DispoAgent
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
        public static Props Props(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Dispo(actorPaths, configuration, hiveConfig, time, debug, principal));
        }
        public Dispo(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, hiveConfig: hiveConfig, time: time, debug: debug, principal: principal)
        {
            DebugMessage(msg: "I'm Alive: " + Context.Self.Path);
            //this.Do(BasicInstruction.Initialize.Create(Self, DispoBehaviour.Get()));
        }
    }
}
