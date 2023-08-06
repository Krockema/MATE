using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hive;
using Akka.Hive.Definitions;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;
using Mate.Production.Core.SignalR;

namespace Mate.Production.Core.Interfaces
{
    public interface ISimulation
    {
        DataBase<MateProductionDb> DbProduction { get; }
        IMessageHub MessageHub { get; }
        SimulationType SimulationType { get; set; }
        bool DebugAgents { get; set; }
        Hive Hive { get; set; }
        IHiveConfig HiveConfig { get; set; }
        ActorPaths ActorPaths { get; set; }
        IActorRef JobCollector { get; set; }
        IActorRef StorageCollector { get; set; }
        IActorRef ContractCollector { get; set; }
        IActorRef ResourceCollector { get; set; }
        IActorRef MeasurementCollector { get; set; }
        IStateManager StateManager { get; set; }
        Task<Hive> InitializeSimulation(Configuration configuration);
    }
}
