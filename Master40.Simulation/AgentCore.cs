using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.ReportingModel;
using Master40.SimulationCore;
using Master40.SimulationCore.Agents;
using Master40.Tools.Messages;
using Master40.Tools.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using static FBreakDowns;

namespace Master40.Simulation
{
    public class AgentCore
    {

        private readonly ProductionDomainContext _context;
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

        public async Task RunAkkaSimulation(Configuration configuration)
        {
            _configuration = configuration;
            _messageHub.SendToAllClients(msg: "Prepare in Memory model from DB for Simulation: " 
                                        + _configuration.GetOption<SimulationId>().Value.ToString()
                                        , msgType: MessageType.info);
            //In-memory database only exists while the connection is open
            //var _inMemory = InMemoryContext.CreateInMemoryContext();
            // InMemoryContext.LoadData(source: _context, target: _inMemory);
            //MasterDBInitializerSimple.DbInitialize(_inMemory);
            
            _messageHub.SendToAllClients(msg: "Prepare Simulation", msgType: MessageType.info);

            _agentSimulation = new AgentSimulation(DBContext: _context
                                                   ,messageHub: _messageHub); // Defines the status output
            
            var simulation = _agentSimulation.InitializeSimulation(configuration: _configuration).Result;
            SimulationContext = simulation.SimulationContext;
 
            
            if (simulation.IsReady())
            {
                _messageHub.SendToAllClients(msg: "Start Simulation ...", msgType: MessageType.info);
                // Start simulation
                var sim = simulation.RunAsync();

                AgentSimulation.Continuation(inbox: _agentSimulation.SimulationConfig.Inbox
                                            , sim: simulation
                                            , collectors: new List<IActorRef> { _agentSimulation.StorageCollector
                                                                , _agentSimulation.WorkCollector
                                                                , _agentSimulation.ContractCollector
                                            });
                await sim;
            }
            _messageHub.EndScheduler();
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
