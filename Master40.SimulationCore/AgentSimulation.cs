using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Reporting;
using Master40.SimulationImmutables;
using Master40.Tools.Simulation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AkkaSim;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.CollectorAgent;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.SupervisorAegnt;
using Master40.Tools.SignalR;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore
{
    public class AgentSimulation
    {
        // public ActorSystem ActorSystem { get; }
        // public IActorRef SimulationContext { get; }
        private List<string> AgentStatistic;
        private readonly IMessageHub _messageHub;
        private readonly bool _debug;
        private readonly ProductionDomainContext _DBContext;
        private AkkaSim.Simulation _simulation;
        public ActorPaths ActorPaths { get; private set; }
        public IActorRef WorkCollector { get; private set; }
        public IActorRef StorageCollector { get; private set; }
        public IActorRef ContractCollector { get; private set; }


        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="debug">Enables AKKA-Global message Debugging</param>
        public AgentSimulation(bool debug, ProductionDomainContext DBContext, IMessageHub messageHub)
        {
            _DBContext = DBContext;
            _messageHub = messageHub;
            _debug = debug;
        }
        public Task<AkkaSim.Simulation> InitializeSimulation(SimulationConfiguration simConfig, SimulationConfig contextConfig)
        {
            return Task.Run(() =>
            {
                OrderGenerator.GenerateOrdersSyncron(_DBContext, simConfig, 1, false); // .RunSynchronously();
                _messageHub.SendToAllClients("Initializing Simulation...");
                var randomWorkTime = new WorkTimeGenerator(simConfig.Seed, simConfig.WorkTimeDeviation, 0);

               // #1 Init Simulation
                _simulation = new AkkaSim.Simulation(contextConfig);
                ActorPaths = new ActorPaths(_simulation.SimulationContext, contextConfig.Inbox.Receiver);
                // Create DataCollector
                WorkCollector = _simulation.ActorSystem.ActorOf(Collector.Props(ActorPaths, CollectorAnalyticsWorkSchedule.Get()
                                                        , _messageHub, _DBContext, 0, false
                                                        , new List<Type> { typeof(CreateSimulationWork),
                                                                                  typeof(UpdateSimulationWork),
                                                                                  typeof(UpdateSimulationWorkProvider),
                                                                                  typeof(UpdateLiveFeed),
                                                                                  typeof(Hub.Instruction.AddMachineToHub),
                                                                                  typeof(BasicInstruction.ResourceBrakeDown)}));
                StorageCollector = _simulation.ActorSystem.ActorOf(Collector.Props(ActorPaths, CollectorAnalyticsStorage.Get()
                                                        , _messageHub, _DBContext, 0, false
                                                        , new List<Type> { typeof(UpdateStockValues),
                                                                                  typeof(UpdateLiveFeed)}));
                ContractCollector = _simulation.ActorSystem.ActorOf(Collector.Props(ActorPaths, CollectorAnalyticsContracts.Get()
                                                        , _messageHub, _DBContext, 0, false
                                                        , new List<Type> { typeof(Contract.Instruction.StartOrder),
                                                                                  typeof(Supervisor.Instruction.OrderProvided),
                                                                                  typeof(UpdateLiveFeed)}));

                // Create Guardians and Inject Childcreators
                var contractGuard = _simulation.ActorSystem.ActorOf(Guardian.Props(ActorPaths, 0, _debug), "ContractGuard");
                var contractBehaveiour = GuardianBehaviour.Get(CreatorOptions.ContractCreator);
                _simulation.SimulationContext.Tell(BasicInstruction.Initialize.Create(contractGuard, contractBehaveiour));

                var dispoGuard = _simulation.ActorSystem.ActorOf(Guardian.Props(ActorPaths, 0, _debug), "DispoGuard");
                var dispoBehaviour = GuardianBehaviour.Get(CreatorOptions.DispoCreator);
                _simulation.SimulationContext.Tell(BasicInstruction.Initialize.Create(dispoGuard, dispoBehaviour));

                var productionGuard = _simulation.ActorSystem.ActorOf(Guardian.Props(ActorPaths, 0, _debug), "ProductionGuard");
                var productionBehaviour = GuardianBehaviour.Get(CreatorOptions.ProductionCreator);
                _simulation.SimulationContext.Tell(BasicInstruction.Initialize.Create(productionGuard, productionBehaviour));

                ActorPaths.AddGuardian(GuardianType.Contract, contractGuard);
                ActorPaths.AddGuardian(GuardianType.Dispo, dispoGuard );
                ActorPaths.AddGuardian(GuardianType.Production, productionGuard);

                // #1.2 Setup DeadLetter Monitor for Debugging
                // var deadletterWatchMonitorProps = Props.Create(() => new DeadLetterMonitor());
                //var deadletterWatchActorRef = _simulation.ActorSystem.ActorOf(deadletterWatchMonitorProps, "DeadLetterMonitoringActor");clockListener
                Action<long> tm = (TimePeriod) => _messageHub.SendToClient("clockListener", TimePeriod.ToString());
                var timeMonitor = Props.Create(() => new TimeMonitor((TimePeriod) => tm(TimePeriod)));
                _simulation.ActorSystem.ActorOf(timeMonitor, "TimeMonitor");
                // subscribe to the event stream for messages of type "DeadLetter"
                // _simulation.ActorSystem.EventStream.Subscribe(deadletterWatchActorRef, typeof(DeadLetter));

                // #2 Create System Agent
                ActorPaths.SetSystemAgent(_simulation.ActorSystem.ActorOf(Supervisor.Props(ActorPaths, 0, _debug, _DBContext, _messageHub, simConfig, ActorRefs.Nobody), "Supervisor"));
                
                // #3 Create DirectoryAgents
                ActorPaths.SetHubDirectoryAgent(_simulation.ActorSystem.ActorOf(Directory.Props(ActorPaths, 0, _debug), "HubDirectory"));
                _simulation.SimulationContext.Tell(BasicInstruction.Initialize.Create(ActorPaths.HubDirectory.Ref, DirectoryBehaviour.Get()));

                ActorPaths.SetStorageDirectory(_simulation.ActorSystem.ActorOf(Directory.Props(ActorPaths, 0, _debug), "StorageDirectory"));
                _simulation.SimulationContext.Tell(BasicInstruction.Initialize.Create(ActorPaths.StorageDirectory.Ref, DirectoryBehaviour.Get()));

                // #4 Create Machines
                foreach (var machine in _DBContext.Machines.Include(m => m.MachineGroup).AsNoTracking())
                {
                    var resource = new FRessourceDefinition(randomWorkTime, machine, _debug);

                    _simulation.SimulationContext.Tell(Directory.Instruction
                                                                .CreateMachineAgents
                                                                .Create(resource, ActorPaths.HubDirectory.Ref)
                                                                , ActorPaths.HubDirectory.Ref);
                }

                // #5 Create Storages
                foreach (var stock in _DBContext.Stocks
                                                .Include(x => x.StockExchanges)
                                                .Include(x => x.Article).ThenInclude(x => x.ArticleToBusinessPartners)
                                                                        .ThenInclude(x => x.BusinessPartner).AsNoTracking()
                                                .Include(x => x.Article).ThenInclude(x => x.ArticleType))
                {
                    _simulation.SimulationContext.Tell(Directory.Instruction
                                                                .CreateStorageAgents
                                                                .Create(stock, ActorPaths.StorageDirectory.Ref)
                                                            , ActorPaths.StorageDirectory.Ref);
                }



                return _simulation;
            });
        }
        public void Run()
        {
           _simulation.RunAsync().Wait();
        }

        public static void Continuation(Inbox inbox, AkkaSim.Simulation sim, List<IActorRef> collectors)
        {

            var something = inbox.ReceiveAsync(System.TimeSpan.FromHours(1)).Result;
            switch (something)
            {
                case SimulationMessage.SimulationState.Started:
                    System.Diagnostics.Debug.WriteLine("AKKA:START AGENT SYSTEM", "AKKA-System:");
                    Continuation(inbox, sim, collectors);
                    break;
                case SimulationMessage.SimulationState.Stopped:
                    System.Diagnostics.Debug.WriteLine("AKKA:STOP AGENT SYSTEM", "AKKA-System:");
                    foreach (var item in collectors)
                    {
                        var waitFor = item.Ask(UpdateLiveFeed.Create(false, inbox.Receiver),TimeSpan.FromHours(1)).Result;
                    }
                    sim.Continue();
                    Continuation(inbox, sim, collectors);
                    break;
                case SimulationMessage.SimulationState.Finished:

                    System.Diagnostics.Debug.WriteLine("SHUTDOWN AGENT SYSTEM", "AKKA-System:");
                    foreach (var item in collectors)
                    {
                        var waitFor = item.Ask(UpdateLiveFeed.Create(true, inbox.Receiver), TimeSpan.FromHours(1)).Result;
                    }
                    sim.ActorSystem.Terminate();
                    break;
                default:
                    break;
            }
        }

    }
}
