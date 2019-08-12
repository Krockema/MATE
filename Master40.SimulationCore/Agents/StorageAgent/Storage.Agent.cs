using System;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using static FArticles;
using static FStockReservations;

namespace Master40.SimulationCore.Agents.StorageAgent
{
    public partial class Storage : Agent
    {

        // Statistic 
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Storage(actorPaths, time, debug, principal));
        }

        public Storage(ActorPaths actorPaths, long time, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            
        }
    }
}
