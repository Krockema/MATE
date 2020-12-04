using Akka.Actor;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Helper;
using static FUpdateStockValues;

namespace Master40.SimulationCore.Agents.StorageAgent
{
    public partial class Storage : Agent
    {

        // Statistic 
        // public Constructor
        public static Props Props(ActorPaths actorPaths, Configuration configuration, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Storage(actorPaths, configuration, time, true, principal));
        }

        public Storage(ActorPaths actorPaths, Configuration configuration, long time, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: principal)
        {
            
        }

        internal void LogValueChange(string article, string articleType, double value)
        {
            var pub = new FUpdateStockValue(stockName: article
                , newValue: value
                , articleType: articleType);
            Context.System.EventStream.Publish(@event: pub);
        }
    }
}
