using Akka.Actor;
using Hangfire;
using Hangfire.Server;
using Master40.DB.Data.Context;
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

        private readonly ProductionDomainContext _context;
        public const string DEFAULT_CONNECTION = "DefaultConnection";
        public const string RESULT_CONNECTION = "ResultConnection";
        private readonly ResultContext _resultContext;
        //private readonly ResultContext _resultContext;
        private AgentSimulation _agentSimulation;
        private Configuration _configuration;
        private IMessageHub _messageHub;
        private IActorRef SimulationContext;
        public AgentCore(ProductionDomainContext context, IMessageHub messageHub)
        {
            _context = context;
            //_resultContext = resultContext;
            _messageHub = messageHub;
        }

        public AgentCore()
        {
            
            _context = ProductionDomainContext.GetContext(ConfigurationManager.AppSettings[DEFAULT_CONNECTION]);
            _resultContext = ResultContext.GetContext(ConfigurationManager.AppSettings[RESULT_CONNECTION]);
        }

        [KeepJobInStore]
        [AutomaticRetry(Attempts = 0)]
        public void BackgroundSimulation(int simulationId, int simNumber, PerformContext consoleContext)
        {
            _messageHub = new ProcessingHub(consoleContext);
            // _messageHub.StartSimulation(simId: simulationId.ToString() , simNumber: simNumber.ToString());
            var simConfig = ArgumentConverter.ConfigurationConverter(_resultContext, simulationId);
            simConfig.AddOption(new DBConnectionString(ConfigurationManager.AppSettings[RESULT_CONNECTION]));
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
            var aggregator = new ResultAggregator(_resultContext);
            aggregator.BuildResults(simulationId);
            _messageHub.SendToAllClients("AggretationFinished for " + simulationId);
        }

        public async Task RunAkkaSimulation(Configuration configuration)
        {
            _configuration = configuration;
            _messageHub.SendToAllClients(msg: "Prepare in Memory model from DB for Simulation: " 
                                        + _configuration.GetOption<SimulationId>().Value);

            _messageHub.SendToAllClients(msg: "Prepare Simulation");

            _agentSimulation = new AgentSimulation(DBContext: _context
                                                   ,messageHub: _messageHub); // Defines the status output
            
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
            var machineGroup = _context.Resources.Include(navigationPropertyPath: x => x.ResourceSkills).Single(predicate: x => x.Name.Replace(" ", "") == name).ResourceSkills.SingleOrDefault().Name;
            SimulationContext.Tell(message: BasicInstruction.ResourceBrakeDown.Create(message: new FBreakDown(resource: "Machine(" + name + ")", resourceSkill: machineGroup, isBroken: true, duration: 0),
                                                                              target: _agentSimulation.ActorPaths.HubDirectory.Ref));
        }
    }
}
