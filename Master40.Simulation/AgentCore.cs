using Akka.Actor;
using Hangfire;
using Hangfire.Server;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.Simulation.CLI;
using Master40.Simulation.HangfireConfiguration;
using Master40.SimulationCore;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.Tools.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using static FBreakDowns;
using Configuration = Master40.SimulationCore.Environment.Configuration;
using ProcessingHub = Master40.Simulation.HangfireConfiguration.ProcessingHub;

namespace Master40.Simulation
{
    public class AgentCore
    {
        private readonly DataBase<ProductionDomainContext> _masterCtx;
        private readonly DataBase<ResultContext> _resultCtx;

        //private readonly ResultContext _resultContext;
        private AgentSimulation _agentSimulation;
        private Configuration _configuration;
        private IMessageHub _messageHub;
        private IActorRef SimulationContext;
        public AgentCore(string dbName, IMessageHub messageHub)
        {
            _masterCtx = Dbms.GetMasterDataBase(dbName: dbName);
            //_resultContext = resultContext;
            _messageHub = messageHub;
        }
        public AgentCore()
        {
            _masterCtx = Dbms.GetMasterDataBase(dbName: "Master40");
            _resultCtx = Dbms.GetResultDataBase(dbName: "Master40");
        }

        [KeepJobInStore]
        [AutomaticRetry(Attempts = 0)]
        public void BackgroundSimulation(int simulationId, int simNumber, PerformContext consoleContext)
        {
            _messageHub = new ProcessingHub(consoleContext);
            // _messageHub.StartSimulation(simId: simulationId.ToString() , simNumber: simNumber.ToString());
            var simConfig = ArgumentConverter.ConfigurationConverter(_resultCtx.DbContext, simulationId);
            simConfig.AddOption(_resultCtx.ConnectionString);
            simConfig.Remove(typeof(SimulationNumber));
            simConfig.AddOption(new SimulationNumber(simNumber));

            RunAkkaSimulation(simConfig).Wait();
            // _messageHub.EndSimulation("Succ", simId: simulationId.ToString() , simNumber: simNumber.ToString());
        }

        
        [KeepJobInStore]
        [AutomaticRetry(Attempts = 0)]
        public void AggregateResults(int simulationId, PerformContext consoleContext)
        {
            _messageHub = new ProcessingHub(consoleContext);
            var aggregator = new ResultAggregator(_resultCtx.DbContext);
            aggregator.BuildResults(simulationId);
            _messageHub.SendToAllClients("AggretationFinished for " + simulationId);
        }

        public async Task RunAkkaSimulation(Configuration configuration)
        {
            _configuration = configuration;
            _messageHub.SendToAllClients(msg: "Prepare in Memory model from DB for Simulation: " 
                                        + _configuration.GetOption<SimulationId>().Value);

            _messageHub.SendToAllClients(msg: "Prepare Simulation");

            _agentSimulation = new AgentSimulation("Master40"
                                                   , messageHub: _messageHub); // Defines the status output
            
            var simulation = await _agentSimulation.InitializeSimulation(configuration: _configuration);
            SimulationContext = simulation.SimulationContext;
 
            
            if (simulation.IsReady())
            {
                _messageHub.StartSimulation(simId: _configuration.GetOption<SimulationId>().Value.ToString()
                                        , simNumber: _configuration.GetOption<SimulationNumber>().Value.ToString());
                    
                // Start simulation
                var sim = simulation.RunAsync();
                _agentSimulation.StateManager.ContinueExecution(simulation);
                await sim;
            }
            _messageHub.EndSimulation(msg: "Simulation Completed."
                                    , simId: _configuration.GetOption<SimulationId>().Value.ToString()
                                    , simNumber: _configuration.GetOption<SimulationNumber>().Value.ToString());
            return;
        }

        public void ResourceBreakDown(string name)
        {
            /*
            var machineGroup = _context.Resources.Include(navigationPropertyPath: x => x.ResourceCapabilities).Single(predicate: x => x.Name.Replace(" ", "") == name).ResourceCapabilities.SingleOrDefault().Name;
            SimulationContext.Tell(message: BasicInstruction.ResourceBrakeDown.Create(message: new FBreakDown(resource: "Machine(" + name + ")", resourceCapability: machineGroup, isBroken: true, duration: 0),
                                                                              target: _agentSimulation.ActorPaths.HubDirectory.Ref));
            */
        }
    }
}
