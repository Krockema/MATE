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
            _messageHub.SendToAllClients("Prepare in Memory model from DB for Simulation: " 
                                        + _configuration.GetOption<SimulationId>().Value.ToString()
                                        , MessageType.info);
            //In-memory database only exists while the connection is open
            var _inMemory = InMemoryContext.CreateInMemoryContext();
            // InMemoryContext.LoadData(_context, _inMemory);
            // MMT 01.07.2019
            //MasterDBInitializerSmall.DbInitialize(_inMemory);

            MasterDBInitializerSimple.DbInitialize(_inMemory);

            PrepareModel(_inMemory);
            
            _messageHub.SendToAllClients("Prepare Simulation", MessageType.info);

            _agentSimulation = new AgentSimulation(_configuration.GetOption<DebugAgents>().Value // Activates Debugging for all Agents
                                                   , DBContext: _inMemory
                                                   ,messageHub: _messageHub); // Defines the status output
            
            var simulation = _agentSimulation.InitializeSimulation(_configuration).Result;
            SimulationContext = simulation.SimulationContext;

            
            
            if (simulation.IsReady())
            {
                _messageHub.SendToAllClients("Start Simulation ...", MessageType.info);
                // Start simulation
                var sim = simulation.RunAsync();

                AgentSimulation.Continuation(_agentSimulation.SimulationConfig.Inbox
                                            , simulation
                                            , new List<IActorRef> { _agentSimulation.StorageCollector
                                                                , _agentSimulation.WorkCollector
                                                                , _agentSimulation.ContractCollector
                                            });
                await sim;
            }
            _messageHub.EndScheduler();
            _messageHub.EndSimulation("Simulation Completed."
                                    , _configuration.GetOption<SimulationId>().Value.ToString()
                                    , _configuration.GetOption<SimulationNumber>().Value.ToString());
            return;
        }

        private void PrepareModel(ProductionDomainContext inMemory)
        {
            inMemory.Resources.RemoveRange(inMemory.Resources.ToList());
            inMemory.SaveChanges();
            inMemory.AddRange(_context.Resources.AsNoTracking().ToList().Select(x => { x.Id = 0; return x; }).ToList());
            inMemory.SaveChanges();
        }

        public void ResourceBreakDown(string name)
        {
            var machineGroup = _context.Resources.Include(x => x.MachineGroup).Single(x => x.Name.Replace(" ", "") == name).MachineGroup.Name;
            SimulationContext.Tell(BasicInstruction.ResourceBrakeDown.Create(message: new SimulationImmutables.FBreakDown("Machine(" + name + ")", machineGroup, true, 0),
                                                                              target: _agentSimulation.ActorPaths.HubDirectory.Ref));
        }
    }
}
