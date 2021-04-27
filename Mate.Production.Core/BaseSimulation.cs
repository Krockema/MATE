using System.Threading.Tasks;
using Akka.Actor;
using AkkaSim;
using AkkaSim.Definitions;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Interfaces;
using Mate.Production.Core.SignalR;

namespace Mate.Production.Core
{
    public abstract class BaseSimulation : ISimulation
    {
        public DataBase<MateProductionDb> DbProduction { get; }
        public IMessageHub MessageHub { get; }
        public SimulationType SimulationType { get; set; }
        public bool DebugAgents { get; set; }
        public Simulation Simulation { get; set; }
        public SimulationConfig SimulationConfig { get; set; }
        public ActorPaths ActorPaths { get; set; }
        public IActorRef JobCollector { get; set; }
        public IActorRef StorageCollector { get; set; }
        public IActorRef ContractCollector { get; set; }
        public IActorRef ResourceCollector { get; set; }
        public IActorRef MeasurementCollector { get; set; }
        public IStateManager StateManager { get; set; }
        public BaseSimulation(string dbName, IMessageHub messageHub)
        {
            DbProduction = Dbms.GetMasterDataBase(dbName: dbName);
            MessageHub = messageHub;
        }

        public abstract Task<Simulation> InitializeSimulation(Configuration configuration);
    }
}
