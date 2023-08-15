using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.StorageAgent
{
    public partial class Storage : Agent
    {

        // Statistic 
        // public Constructor
        public static Props Props(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Storage(actorPaths, configuration, hiveConfig, time, true, principal));
        }

        public Storage(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, Time time, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, hiveConfig: hiveConfig, time: time, debug: debug, principal: principal)
        {
            
        }

        internal void LogValueChange(string article, string articleType, double value)
        {
            var pub = new UpdateStockValueRecord(StockName: article
                , NewValue: value
                , ArticleType: articleType);
            Context.System.EventStream.Publish(@event: pub);
        }
    }
}
