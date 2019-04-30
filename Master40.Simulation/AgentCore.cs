using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;
using Master40.SimulationCore;
using Master40.SimulationCore.Agents;
using Master40.Tools.Messages;
using Master40.Tools.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Master40.Simulation
{
    public class AgentCore
    {

        private readonly ProductionDomainContext _context;
        private readonly ResultContext _resultContext;
        private AgentSimulation _agentSimulation;
        private IMessageHub _messageHub;
        private IActorRef SimulationContext;
        public AgentCore(ProductionDomainContext context, ResultContext resultContext, IMessageHub messageHub)
        {
            _context = context;
            _resultContext = resultContext;
            _messageHub = messageHub;
        }

        public async Task RunAkkaSimulation(SimulationConfiguration simConfig)
        {
            _messageHub.SendToAllClients("Prepare in Memory model from DB for Simulation: " + simConfig.Id, MessageType.info);
            //In-memory database only exists while the connection is open
            var _inMemory = InMemoryContext.CreateInMemoryContext();
            // InMemoryContext.LoadData(_context, _inMemory);
            MasterDBInitializerSmall.DbInitialize(_inMemory);
            PrepareModel(_context, _inMemory);


            var simNumber = _resultContext.GetSimulationNumber(simConfig.Id, SimulationType.Decentral);
            
            _messageHub.SendToAllClients("Prepare Simulation", MessageType.info);
            _agentSimulation = new AgentSimulation(false, _inMemory, _resultContext, _messageHub);
            
            var simModelConfig = new SimulationConfig(false, simConfig.DynamicKpiTimeSpan);
            var simulation = await _agentSimulation.InitializeSimulation(simConfig, simModelConfig);
            SimulationContext = simulation.SimulationContext;
            if (simulation.IsReady())
            {
                _messageHub.SendToAllClients("Start Simulation ...", MessageType.info);
                // Start simulation
                var sim = simulation.RunAsync();

                AgentSimulation.Continuation(simModelConfig.Inbox
                                            , simulation
                                            , new List<IActorRef> { _agentSimulation.StorageCollector
                                                                , _agentSimulation.WorkCollector
                                                                , _agentSimulation.ContractCollector
                                            });
                await sim;


                // var ws = _inMemory.SimulationWorkschedules.AsNoTracking().ToList().Select(x => { x.Id = 0; return x; }).ToList();
                // _context.SimulationWorkschedules.AddRange(ws);
                    
                // _context.SaveChanges();
            }
            _messageHub.EndScheduler();

            // CopyResults.Copy(c, _evaluationContext, simulationConfigurationId, simNumber, SimulationType.Decentral);
            // var simConfig = _evaluationContext.SimulationConfigurations.Single(x => x.Id == simulationConfigurationId);
            // CalculateKpis.MachineSattleTime(_evaluationContext, simConfig, SimulationType.Decentral, simNumber);
            // 
            // CalculateKpis.CalculateAllKpis(_evaluationContext, simulationConfigurationId, SimulationType.Decentral, simNumber, true);



            _context.Database.CloseConnection();
            _messageHub.EndSimulation("Simulation with Id:" + _context + " Completed."
                                            , _context.ToString()
                                            , simNumber.ToString());
        }

        public SimulationConfiguration UpdateSettings(int simId, int orderAmount, double arivalRate, int estimatedThroughputTime)
        {
            var simConfig = _resultContext.SimulationConfigurations.AsNoTracking().Single(x => x.Id == simId);
            simConfig.OrderQuantity = orderAmount;
            simConfig.OrderRate = arivalRate;
            // TODO: Create an own Field for it in the model
            simConfig.Time = estimatedThroughputTime;
            return simConfig;
        }

        private void PrepareModel(ProductionDomainContext context, ProductionDomainContext inMemory)
        {
            inMemory.Machines.RemoveRange(inMemory.Machines.ToList());
            inMemory.SaveChanges();
            inMemory.AddRange(_context.Machines.AsNoTracking().ToList().Select(x => { x.Id = 0; return x; }).ToList());
            inMemory.SaveChanges();
        }

        public void ResourceBreakDown(string name)
        {
            var machineGroup = _context.Machines.Include(x => x.MachineGroup).Single(x => x.Name.Replace(" ", "") == name).MachineGroup.Name;
            SimulationContext.Tell(BasicInstruction.ResourceBrakeDown.Create(message: new SimulationImmutables.FBreakDown("Machine(" + name + ")", machineGroup, true, 0),
                                                                              target: _agentSimulation.ActorPaths.HubDirectory.Ref));
        }
    }
}
