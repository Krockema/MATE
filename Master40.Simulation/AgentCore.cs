using System;
using Akka.Actor;
using Master40.DB.Data.Context;
using Master40.SimulationCore;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Environment.Options;
using Master40.Tools.Messages;
using Master40.Tools.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AkkaSim.Logging;
using Hangfire;
using Master40.Simulation.CLI;
using Master40.Simulation.HangfireConfiguration;
using NLog;
using static FBreakDowns;
using Configuration = Master40.SimulationCore.Environment.Configuration;

namespace Master40.Simulation
{
    public class AgentCore
    {

        private readonly ProductionDomainContext _context;
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
            _messageHub = new ProcessingHub();
            _context = ProductionDomainContext.GetContext(ConfigurationManager.AppSettings[index: 0]);
            _resultContext = ResultContext.GetContext(ConfigurationManager.AppSettings[index: 2]);
        }

        [KeepJobInStore]
        [AutomaticRetry(Attempts = 0)]
        public async Task BackgroundSimulation(int simulationId, int simNumber)
        {
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            _messageHub.StartSimulation(simId: simulationId.ToString() , simNumber: simNumber.ToString());
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            var simConfig = ArgumentConverter.ConfigurationConverter(_resultContext, simulationId);
            _messageHub.EndSimulation("Succ", simId: simulationId.ToString() , simNumber: simNumber.ToString());
            // await RunAkkaSimulation(simConfig);
        }

        public async Task RunAkkaSimulation(Configuration configuration)
        {
            LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AGENTS, LogLevel.Debug);
            LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_AGENTS, LogLevel.Warn);


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
                //AgentSimulation.Continuation(inbox: _agentSimulation.SimulationConfig.Inbox
                //                            , sim: simulation
                //                            , collectors: new List<IActorRef> { _agentSimulation.StorageCollector
                //                                                , _agentSimulation.WorkCollector
                //                                                , _agentSimulation.ContractCollector
                //                            });
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
