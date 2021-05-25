using System.Threading.Tasks;
using Akka.Actor;
using Hangfire;
using Hangfire.Server;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.Production.CLI.HangfireConfiguration;
using Mate.Production.Core;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Helper;
using Mate.Production.Core.SignalR;
using Configuration = Mate.Production.Core.Environment.Configuration;
using ProcessingHub = Mate.Production.CLI.HangfireConfiguration.ProcessingHub;

namespace Mate.Production.CLI
{
    public class AgentCore
    {
        private readonly DataBase<MateProductionDb> _mateCtx;
        private readonly DataBase<MateResultDb> _resultCtx;

        //private readonly ResultContext _resultContext;
        private AgentSimulation _agentSimulation;
        private Configuration _configuration;
        private IMessageHub _messageHub;
        private IActorRef SimulationContext;
        public AgentCore(string dbName, IMessageHub messageHub)
        {
            _mateCtx = Dbms.GetMateDataBase(dbName: dbName);
            //_resultContext = resultContext;
            _messageHub = messageHub;
        }
        public AgentCore(IMessageHub messageHub)
        {
            _messageHub = messageHub;
            _mateCtx = Dbms.GetMateDataBase(dbName: DataBaseConfiguration.MateDb);
            _resultCtx = Dbms.GetResultDataBase(dbName: DataBaseConfiguration.MateResultDb);
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

            _agentSimulation = new AgentSimulation(DataBaseConfiguration.MateDb
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
