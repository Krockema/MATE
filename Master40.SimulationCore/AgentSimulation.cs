using Akka.Actor;
using Akka.Event;
using AkkaSim;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Agents.CollectorAgent;
using Master40.SimulationCore.Agents.CollectorAgent.Types;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Agents.Guardian.Options;
using Master40.SimulationCore.Agents.SupervisorAgent;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Reporting;
using Master40.Tools.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FResourceSetupDefinitions;
using static FSetEstimatedThroughputTimes;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore
{
    public class AgentSimulation
    {
        // public ActorSystem ActorSystem { get; }
        // public IActorRef SimulationContext { get; }
        private ProductionDomainContext _dBContext { get; }
        private IMessageHub _messageHub { get; }
        private SimulationType _simulationType { get; set; }
        private bool _debugAgents { get; set; }
        private Simulation _simulation { get; set; }
        public SimulationConfig SimulationConfig { get; private set; }
        public ActorPaths ActorPaths { get; private set; }
        public IActorRef WorkCollector { get; private set; }
        public IActorRef StorageCollector { get; private set; }
        public IActorRef ContractCollector { get; private set; }
        public static LogWriter Logger { get; set; }

        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="debug">Enables AKKA-Global message Debugging</param>
        public AgentSimulation(ProductionDomainContext DBContext, IMessageHub messageHub)
        {
            _dBContext = DBContext;
            _messageHub = messageHub;
        }
        public Task<Simulation> InitializeSimulation(Configuration configuration)
        {
            return Task.Run(function: () =>
            {
                _messageHub.SendToAllClients(msg: "Initializing Simulation...");
                // Init Simulation
                SimulationConfig = configuration.GetContextConfiguration();
                _debugAgents = configuration.GetOption<DebugAgents>().Value;
                Logger = new LogWriter(writeToFile: _debugAgents);
                _simulationType = configuration.GetOption<SimulationKind>().Value;
                _simulation = new Simulation(simConfig: SimulationConfig);
                ActorPaths = new ActorPaths(simulationContext: _simulation.SimulationContext, systemMailBox: SimulationConfig.Inbox.Receiver);

                // Create DataCollectors
                CreateCollectorAgents(configuration: configuration);
                if (_debugAgents) AddDeadLetterMonitor();
                AddTimeMonitor();

                // Create Guardians and Inject Childcreators
                GenerateGuardians();

                // Create Supervisor Agent
                CreateSupervisor(configuration: configuration);

                // Create DirectoryAgents
                CreateDirectoryAgents();

                // Create Resources
                CreateResourceAgents(configuration: configuration);

                // Create Storages
                CreateStorageAgents();

                return _simulation;
            });
        }

        private void CreateSupervisor(Configuration configuration)
        {
            var products = _dBContext.GetProducts();
            var initialTime = configuration.GetOption<EstimatedThroughPut>().Value;

            var estimatedThroughputs = products.Select(a => new FSetEstimatedThroughputTime(a.Id, initialTime, a.Name))
                                               .ToList();


            ActorPaths.SetSupervisorAgent(systemAgent: _simulation.ActorSystem
                .ActorOf(props: Supervisor.Props(actorPaths: ActorPaths,
                        time: 0,
                        debug: _debugAgents,
                        productionDomainContext: _dBContext,
                        messageHub: _messageHub,
                        configuration: configuration,
                        estimatedThroughputTimes: estimatedThroughputs,
                        principal: ActorRefs.Nobody),
                    name: "Supervisor"));
        }

        private void CreateDirectoryAgents()
        {
            ActorPaths.SetHubDirectoryAgent(hubAgent: _simulation.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, time: 0, debug: _debugAgents), name: "HubDirectory"));
            _simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.HubDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: _simulationType)));

            ActorPaths.SetStorageDirectory(storageAgent: _simulation.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, time: 0, debug: _debugAgents), name: "StorageDirectory"));
            _simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.StorageDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: _simulationType)));
        }

        private void CreateCollectorAgents(Configuration configuration)
        {
            var resourcelist = new ResourceList();
            resourcelist.AddRange(collection: _dBContext.Resources.Select(selector: x => "Resources(" + x.Name.Replace(" ", "") + ")"));

            StorageCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsStorage.Get()
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticsStorage.GetStreamTypes()), name: "StorageCollector");
            ContractCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsContracts.Get()
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticsContracts.GetStreamTypes()), name: "ContractCollector");
            WorkCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsWorkSchedule.Get(resources: resourcelist)
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticsWorkSchedule.GetStreamTypes()), name: "WorkScheduleCollector");
        }

        private void GenerateGuardians()
        {
            CreateGuard(guardianType: GuardianType.Contract, guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.ContractCreator, simulationType: _simulationType));
            CreateGuard(guardianType: GuardianType.Dispo, guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.DispoCreator, simulationType: _simulationType));
            CreateGuard(guardianType: GuardianType.Production, guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.ProductionCreator, simulationType: _simulationType));
        }

        private void CreateGuard(GuardianType guardianType, GuardianBehaviour guardianBehaviour)
        {
            var guard = _simulation.ActorSystem.ActorOf(props: Guardian.Props(actorPaths: ActorPaths, time: 0, debug: _debugAgents), name: guardianType.ToString() + "Guard");
            _simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: guard, message: guardianBehaviour));
            ActorPaths.AddGuardian(guardianType: guardianType, actorRef: guard);
        }

        /// <summary>
        /// Creates an Time Agent that is listening to Clock AdvanceTo messages and serves the frontend with current time updates
        /// </summary>
        private void AddTimeMonitor()
        {
            Action<long> tm = (timePeriod) => _messageHub.SendToClient(listener: "clockListener", msg: timePeriod.ToString());
            var timeMonitor = Props.Create(factory: () => new TimeMonitor((timePeriod) => tm(timePeriod)));
            _simulation.ActorSystem.ActorOf(props: timeMonitor, name: "TimeMonitor");
        }

        private void AddDeadLetterMonitor()
        {
            var deadletterWatchMonitorProps = Props.Create(factory: () => new DeadLetterMonitor());
            var deadletterWatchActorRef = _simulation.ActorSystem.ActorOf(props: deadletterWatchMonitorProps, name: "DeadLetterMonitoringActor");
            //subscribe to the event stream for messages of type "DeadLetter"
            _simulation.ActorSystem.EventStream.Subscribe(subscriber: deadletterWatchActorRef, channel: typeof(DeadLetter));
        }

        /// <summary>
        /// Creates StorageAgents based on current Model and add them to the SimulationContext
        /// </summary>
        private void CreateStorageAgents()
        {
            foreach (var stock in _dBContext.Stocks
                                            .Include(navigationPropertyPath: x => x.StockExchanges)
                                            .Include(navigationPropertyPath: x => x.Article).ThenInclude(navigationPropertyPath: x => x.ArticleToBusinessPartners)
                                                                    .ThenInclude(navigationPropertyPath: x => x.BusinessPartner)
                                            .Include(navigationPropertyPath: x => x.Article).ThenInclude(navigationPropertyPath: x => x.ArticleType)
                                            .AsNoTracking().ToList())
            {
                _simulation.SimulationContext.Tell(message: Directory.Instruction
                                                            .CreateStorageAgents
                                                            .Create(message: stock, target: ActorPaths.StorageDirectory.Ref)
                                                        , sender: ActorPaths.StorageDirectory.Ref);
            }
        }

        /// <summary>
        /// Creates ResourcesAgents based on current Model and add them to the SimulationContext
        /// </summary>
        /// <param name="configuration">Environment.Configuration</param>
        private void CreateResourceAgents(Configuration configuration)
        {
            WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration: configuration);

            var setups = _dBContext.ResourceSetups.Include(navigationPropertyPath: m => m.Resource)
                                                                 .Include(navigationPropertyPath: r => r.ResourceSkill)
                                                                    .ThenInclude(s => s.ResourceSetups)
                                                                 .Include(navigationPropertyPath: t => t.ResourceTool)
                                                                 .AsNoTracking().ToListAsync().Result;

            var resourceList = _dBContext.Resources.ToList();

            foreach (var resource in resourceList)
            {
                var resourceSetups = setups.Where(predicate: x => x.ResourceId == resource.Id).ToList();

                var resourceSetupDefinition = new FResourceSetupDefinition(workTimeGenerator: randomWorkTime, resourceSetup: resourceSetups, debug: _debugAgents);

                _simulation.SimulationContext.Tell(message: Directory.Instruction
                                                            .CreateMachineAgents
                                                            .Create(message: resourceSetupDefinition, target: ActorPaths.HubDirectory.Ref)
                                                            , sender: ActorPaths.HubDirectory.Ref);
            }
        }

        public static void Continuation(Inbox inbox, Simulation sim, List<IActorRef> collectors)
        {

            var something = inbox.ReceiveAsync(timeout: TimeSpan.FromHours(value: 1)).Result;
            switch (something)
            {
                case SimulationMessage.SimulationState.Started:
                    System.Diagnostics.Debug.WriteLine(message: "AKKA:START AGENT SYSTEM", category: "AKKA-System:");
                    Continuation(inbox: inbox, sim: sim, collectors: collectors);
                    break;
                case SimulationMessage.SimulationState.Stopped:
                    System.Diagnostics.Debug.WriteLine(message: "AKKA:STOP AGENT SYSTEM", category: "AKKA-System:");
                    var tasks = new List<Task>();
                    tasks.Add(Logger.WriteToFile());
                    foreach (var item in collectors)
                    {
                        var msg = UpdateLiveFeed.Create(setup: false, target: inbox.Receiver);
                        System.Diagnostics.Debug.WriteLine($"Ask for Update Feed {item.Path.Name}");
                        tasks.Add(item.Ask(message: msg, timeout: TimeSpan.FromSeconds(value: 60 * 60)));
                        
                    }
                    Task.WaitAll(tasks.ToArray());
                    sim.Continue();
                    Continuation(inbox: inbox, sim: sim, collectors: collectors);
                    break;
                case SimulationMessage.SimulationState.Finished:
                    System.Diagnostics.Debug.WriteLine(message: "SHUTDOWN AGENT SYSTEM", category: "AKKA-System:");
                    foreach (var item in collectors)
                    {
                        var waitFor = item.Ask(message: UpdateLiveFeed.Create(setup: true, target: inbox.Receiver), timeout: TimeSpan.FromHours(value: 1)).Result;
                    }
                    sim.ActorSystem.Terminate().Wait();
                    break;
                default:
                    break;
            }
        }

    }
}
