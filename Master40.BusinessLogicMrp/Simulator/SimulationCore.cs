using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.MessageSystem.SignalR;
using Master40.SimulationCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Initializer;

namespace Master40.BusinessLogicCentral.Simulator
{
    public class AgentCore
    {

        private readonly ProductionDomainContext _context;
        private AgentSimulation _agentSimulation;
        private IMessageHub _messageHub;
        public AgentCore(ProductionDomainContext context, IMessageHub messageHub)
        {
            _context = context;
            _messageHub = messageHub;
        }

        public async Task RunAkkaSimulation(int simulationId, int simulationNumber)
        {
            _messageHub.SendToAllClients("Prepare in Memory model from DB", MessageSystem.Messages.MessageType.info);
            //In-memory database only exists while the connection is open
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());

            // create OptionsBuilder with InMemmory Context
            var builder = new DbContextOptionsBuilder<MasterDBContext>();
            builder.UseSqlite(connection);
            var simNumber = _context.GetSimulationNumber(simulationId, SimulationType.Decentral);
            var simConfig = _context.SimulationConfigurations.Single(x => x.Id == simulationId);

            using (var c = new InMemoryContext(builder.Options))
            {
                c.Database.OpenConnection();
                c.Database.EnsureCreated();
                //InMemoryContext.LoadData(_context, c);
                MasterDBInitializerLarge.DbInitialize(c);

                _messageHub.SendToAllClients("Prepare Simulation", MessageSystem.Messages.MessageType.info);
                _agentSimulation = new AgentSimulation(false, c, _messageHub);

                var simModelConfig = new SimulationConfig(false, simConfig.DynamicKpiTimeSpan);
                var simulation = await _agentSimulation.InitializeSimulation(simConfig, simModelConfig);

                if (simulation.IsReady())
                {
                    _messageHub.SendToAllClients("Start Simulation ...", MessageSystem.Messages.MessageType.info);
                    // Start simulation
                    var sim = simulation.RunAsync();

                    AgentSimulation.Continuation(simModelConfig.Inbox
                                                , simulation
                                                , new List<IActorRef> { _agentSimulation.StorageCollector
                                                                    , _agentSimulation.WorkCollector
                                                                    , _agentSimulation.ContractCollector
                                                });
                    await sim;


                    var ws = c.SimulationWorkschedules.AsNoTracking().ToList().Select(x => { x.Id = 0; return x; }).ToList();
                    _context.SimulationWorkschedules.AddRange(ws);
                    
                    _context.SaveChanges();
                }
                _messageHub.EndScheduler();

                // CopyResults.Copy(c, _evaluationContext, simulationConfigurationId, simNumber, SimulationType.Decentral);
                // var simConfig = _evaluationContext.SimulationConfigurations.Single(x => x.Id == simulationConfigurationId);
                // CalculateKpis.MachineSattleTime(_evaluationContext, simConfig, SimulationType.Decentral, simNumber);
                // 
                // CalculateKpis.CalculateAllKpis(_evaluationContext, simulationConfigurationId, SimulationType.Decentral, simNumber, true);
            }
            connection.Close();
            _messageHub.EndSimulation("Simulation with Id:" + _context + " Completed."
                                            , _context.ToString()
                                            , simNumber.ToString());





        }
    }
}
