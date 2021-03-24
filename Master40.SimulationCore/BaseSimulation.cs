using Akka.Actor;
using AkkaSim;
using AkkaSim.Definitions;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Nominal;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using Master40.Tools.SignalR;
using System.Threading.Tasks;

namespace Master40.SimulationCore
{
    public abstract class BaseSimulation : ISimulation
    {
        public DataBase<ProductionDomainContext> DbProduction { get; }
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
