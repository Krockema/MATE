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
            return Task.Run(() =>
            {
                _messageHub.SendToAllClients("Initializing Simulation...");
                // Init Simulation
                SimulationConfig = configuration.GetContextConfiguration();
                _debugAgents = configuration.GetOption<DebugAgents>().Value;
                _simulationType = configuration.GetOption<SimulationKind>().Value;
                _simulation = new Simulation(SimulationConfig);
                ActorPaths = new ActorPaths(_simulation.SimulationContext, SimulationConfig.Inbox.Receiver);

                // Create DataCollectors
                CreateCollectorAgents(configuration);
                if (_debugAgents) AddDeadLetterMonitor();
                AddTimeMonitor();

                // Create Guardians and Inject Childcreators
                GenerateGuadians();

                // Create Supervisor Agent
                var productIds = _dBContext.GetProductIds();
                ActorPaths.SetSupervisorAgent(_simulation.ActorSystem.ActorOf(Supervisor.Props(ActorPaths, 0, _debugAgents, _dBContext, _messageHub, configuration, productIds, ActorRefs.Nobody), "Supervisor"));

                // Create DirectoryAgents
                CreateDirectoryAgents();

                // Create Resources
                CreateResourceAgents(configuration);

                // Create Storages
                CreateStorageAgents();

                return _simulation;
            });
        }

        private void CreateDirectoryAgents()
        {
            ActorPaths.SetHubDirectoryAgent(_simulation.ActorSystem.ActorOf(Directory.Props(ActorPaths, 0, _debugAgents), "HubDirectory"));
            _simulation.SimulationContext.Tell(BasicInstruction.Initialize.Create(ActorPaths.HubDirectory.Ref, Agents.DirectoryAgent.Behaviour.Factory.Get(_simulationType)));

            ActorPaths.SetStorageDirectory(_simulation.ActorSystem.ActorOf(Directory.Props(ActorPaths, 0, _debugAgents), "StorageDirectory"));
            _simulation.SimulationContext.Tell(BasicInstruction.Initialize.Create(ActorPaths.StorageDirectory.Ref, Agents.DirectoryAgent.Behaviour.Factory.Get(_simulationType)));
        }

        private void CreateCollectorAgents(Configuration configuration)
        {
            var resourcelist = new ResourceList();
            resourcelist.AddRange(_dBContext.Resources.Select(x => "Resources(" + x.Name.Replace(" ", "") + ")"));

            WorkCollector = _simulation.ActorSystem.ActorOf(Collector.Props(ActorPaths, CollectorAnalyticsWorkSchedule.Get(resourcelist)
                                                            , _messageHub, configuration, 0, _debugAgents
                                                            , CollectorAnalyticsWorkSchedule.GetStreamTypes()));
            StorageCollector = _simulation.ActorSystem.ActorOf(Collector.Props(ActorPaths, CollectorAnalyticsStorage.Get()
                                                            , _messageHub, configuration, 0, _debugAgents
                                                            , CollectorAnalyticsStorage.GetStreamTypes()));
            ContractCollector = _simulation.ActorSystem.ActorOf(Collector.Props(ActorPaths, CollectorAnalyticsContracts.Get()
                                                            , _messageHub, configuration, 0, _debugAgents
                                                            , CollectorAnalyticsContracts.GetStreamTypes()));
        }

        private void GenerateGuadians()
        {
            CreateGuard(GuardianType.Contract, GuardianBehaviour.Get(ChildMaker.ContractCreator, _simulationType));
            CreateGuard(GuardianType.Dispo, GuardianBehaviour.Get(ChildMaker.DispoCreator, _simulationType));
            CreateGuard(GuardianType.Production, GuardianBehaviour.Get(ChildMaker.ProductionCreator, _simulationType));
        }

        private void CreateGuard(GuardianType guardianType, GuardianBehaviour guardianBehaviour)
        {
            var guard = _simulation.ActorSystem.ActorOf(Guardian.Props(ActorPaths, 0, _debugAgents), guardianType.ToString() + "Guard");
            _simulation.SimulationContext.Tell(BasicInstruction.Initialize.Create(guard, guardianBehaviour));
            ActorPaths.AddGuardian(guardianType, guard);
        }

        /// <summary>
        /// Creates an Time Agent that is listening to Clock AdvanceTo messages and serves the frontend with current time updates
        /// </summary>
        private void AddTimeMonitor()
        {
            Action<long> tm = (timePeriod) => _messageHub.SendToClient("clockListener", timePeriod.ToString());
            var timeMonitor = Props.Create(() => new TimeMonitor((timePeriod) => tm(timePeriod)));
            _simulation.ActorSystem.ActorOf(timeMonitor, "TimeMonitor");
        }

        private void AddDeadLetterMonitor()
        {
            var deadletterWatchMonitorProps = Props.Create(() => new DeadLetterMonitor());
            var deadletterWatchActorRef = _simulation.ActorSystem.ActorOf(deadletterWatchMonitorProps, "DeadLetterMonitoringActor");
            //subscribe to the event stream for messages of type "DeadLetter"
            _simulation.ActorSystem.EventStream.Subscribe(deadletterWatchActorRef, typeof(DeadLetter));
        }

        /// <summary>
        /// Creates StorageAgents based on current Model and add them to the SimulationContext
        /// </summary>
        private void CreateStorageAgents()
        {
            foreach (var stock in _dBContext.Stocks
                                            .Include(x => x.StockExchanges)
                                            .Include(x => x.Article).ThenInclude(x => x.ArticleToBusinessPartners)
                                                                    .ThenInclude(x => x.BusinessPartner)
                                            .Include(x => x.Article).ThenInclude(x => x.ArticleType)
                                            .AsNoTracking().ToList())
            {
                _simulation.SimulationContext.Tell(Directory.Instruction
                                                            .CreateStorageAgents
                                                            .Create(stock, ActorPaths.StorageDirectory.Ref)
                                                        , ActorPaths.StorageDirectory.Ref);
            }
        }

        /// <summary>
        /// Creates ResourcesAgents based on current Model and add them to the SimulationContext
        /// </summary>
        /// <param name="configuration">Environment.Configuration</param>
        private void CreateResourceAgents(Configuration configuration)
        {
            WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration);

            var setups = _dBContext.ResourceSetups.Include(m => m.Resource)
                                                                 .Include(r => r.ResourceSkill)
                                                                 .Include(t => t.ResourceTool)
                                                                 .AsNoTracking().ToListAsync().Result;

            var resourceList = _dBContext.Resources.ToList();

            foreach (var resource in resourceList)
            {
                var resourceSetups = setups.Where(x => x.ResourceId == resource.Id).ToList();

                var resourceSetupDefinition = new FResourceSetupDefinition(randomWorkTime, resourceSetups, _debugAgents);

                _simulation.SimulationContext.Tell(Directory.Instruction
                                                            .CreateMachineAgents
                                                            .Create(resourceSetupDefinition, ActorPaths.HubDirectory.Ref)
                                                            , ActorPaths.HubDirectory.Ref);
            }
        }

        public static void Continuation(Inbox inbox, Simulation sim, List<IActorRef> collectors)
        {

            var something = inbox.ReceiveAsync(TimeSpan.FromHours(1)).Result;
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
