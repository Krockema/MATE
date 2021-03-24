using Akka.Actor;
using AkkaSim;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Nominal;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Helper;
using Master40.Tools.SignalR;
using System.Threading.Tasks;

namespace Master40.SimulationCore.Types
{
    public interface ISimulation
    {
        DataBase<ProductionDomainContext> DbProduction { get; }
        IMessageHub MessageHub { get; }
        SimulationType SimulationType { get; set; }
        bool DebugAgents { get; set; }
        Simulation Simulation { get; set; }
        SimulationConfig SimulationConfig { get; set; }
        ActorPaths ActorPaths { get; set; }
        IActorRef JobCollector { get; set; }
        IActorRef StorageCollector { get; set; }
        IActorRef ContractCollector { get; set; }
        IActorRef ResourceCollector { get; set; }
        IActorRef MeasurementCollector { get; set; }
        IStateManager StateManager { get; set; }
        Task<Simulation> InitializeSimulation(Configuration configuration);
    }
}
