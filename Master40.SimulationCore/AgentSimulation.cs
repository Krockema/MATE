using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using AkkaSim;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Agents.CollectorAgent;
using Master40.SimulationCore.Agents.CollectorAgent.Types;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Agents.Guardian.Options;
using Master40.SimulationCore.Agents.SupervisorAgent;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.SimulationCore.Reporting;
using Master40.Tools.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using System.Threading.Tasks;
using AkkaSim.SpecialActors;
using Master40.SimulationCore.Types;
using static FCapabilityProviderDefinitions;
using static FSetEstimatedThroughputTimes;

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
        public IActorRef JobCollector { get; private set; }
        public IActorRef StorageCollector { get; private set; }
        public IActorRef ContractCollector { get; private set; }
        public IActorRef ResourceCollector { get; private set; }
        public AgentStateManager StateManager { get; private set; }
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
                _simulationType = configuration.GetOption<SimulationKind>().Value;
                _simulation = new Simulation(simConfig: SimulationConfig);
                ActorPaths = new ActorPaths(simulationContext: _simulation.SimulationContext, systemMailBox: SimulationConfig.Inbox.Receiver);

                // Create DataCollectors
                CreateCollectorAgents(configuration: configuration);
                //if (_debugAgents) 
                AddDeadLetterMonitor();
                AddTimeMonitor();

                // Create Guardians and Inject Childcreators
                GenerateGuardians();

                // Create Supervisor Agent
                CreateSupervisor(configuration: configuration);

                // Create DirectoryAgents
                CreateDirectoryAgents(configuration: configuration);

                // Create Resources
                CreateResourceAgents(configuration: configuration);

                // Create Storages
                CreateStorageAgents();

                // Finally Initialize StateManger
                StateManager = new AgentStateManager(new List<IActorRef> { this.StorageCollector
                                                                         , this.JobCollector
                                                                         , this.ContractCollector
                                                                         , this.ResourceCollector
                                                                     }
                                                    , SimulationConfig.Inbox);

                return _simulation;
            });
        }

        private void CreateSupervisor(Configuration configuration)
        {
            var products = _dBContext.GetProducts();
            var initialTime = configuration.GetOption<EstimatedThroughPut>().Value;

            var estimatedThroughPuts = products.Select(a => new FSetEstimatedThroughputTime(a.Id, initialTime, a.Name))
                .ToList();

            var behave = Agents.SupervisorAgent.Behaviour.Factory.Default(
                                    productionDomainContext: _dBContext,
                                    messageHub: _messageHub,
                                    configuration: configuration,
                                    estimatedThroughputTimes: estimatedThroughPuts);
            ActorPaths.SetSupervisorAgent(systemAgent: _simulation.ActorSystem
                .ActorOf(props: Supervisor.Props(actorPaths: ActorPaths,
                        time: 0,
                        debug: _debugAgents,
                        principal: ActorRefs.Nobody),
                    name: "Supervisor"));
            _simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.SystemAgent.Ref, message: behave));
        }

        private void CreateDirectoryAgents(Configuration configuration)
        {
            // Resource Directory
            ActorPaths.SetHubDirectoryAgent(hubAgent: _simulation.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, time: 0, debug: _debugAgents), name: "HubDirectory"));
            _simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.HubDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: _simulationType)));
            CreateHubAgents(ActorPaths.HubDirectory.Ref, configuration);

            // Storage Directory
            ActorPaths.SetStorageDirectory(storageAgent: _simulation.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, time: 0, debug: _debugAgents), name: "StorageDirectory"));
            _simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.StorageDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: _simulationType)));
        }

        private void CreateHubAgents(IActorRef directory, Configuration configuration)
        {
           
            var capabilities = _dBContext.ResourceCapabilities.Where(x => x.ParentResourceCapabilityId == null).ToList();
            for (var index = 0; index < capabilities.Count; index++)
            {
                WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration: configuration, index);
                var capability = capabilities[index];
                GetCapabilitiesRecursive(capability);

                var hubInfo = new FResourceHubInformations.FResourceHubInformation(capability: capability
                                                                           ,workTimeGenerator: randomWorkTime
                                                                              , maxBucketSize: configuration.GetOption<MaxBucketSize>().Value);
                _simulation.SimulationContext.Tell(
                    message: Directory.Instruction.Default.CreateResourceHubAgents.Create(hubInfo, directory),
                    sender: ActorRefs.NoSender);
            }
        }

        private void GetCapabilitiesRecursive(M_ResourceCapability capability)
        {
            capability.ChildResourceCapabilities = _dBContext.ResourceCapabilities.Where(x => x.ParentResourceCapabilityId == capability.Id).ToList();
            capability.ChildResourceCapabilities.ForEach(GetCapabilitiesRecursive);
        }

        private void CreateCollectorAgents(Configuration configuration)
        {
            var resourcelist = new ResourceDictionary();
            _dBContext.Resources.Where(x => x.IsPhysical)
                                .Select(selector: x => new {x.Id, Name = x.Name.Replace(" ", "") })
                                .ForEach(x => resourcelist.Add(x.Id, x.Name));

            StorageCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsStorage.Get()
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticsStorage.GetStreamTypes()), name: "StorageCollector");
            ContractCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsContracts.Get()
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticsContracts.GetStreamTypes()), name: "ContractCollector");
            JobCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticJob.Get(resources: resourcelist)
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticJob.GetStreamTypes()), name: "JobCollector");
            ResourceCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticResource.Get(resources: resourcelist)
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticResource.GetStreamTypes()), name: "ResourceCollector");
        }

        private void GenerateGuardians()
        {
            CreateGuard(guardianType: GuardianType.Contract, guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.ContractCreator, simulationType: _simulationType, _messageHub));
            CreateGuard(guardianType: GuardianType.Dispo, guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.DispoCreator, simulationType: _simulationType,_messageHub));
            CreateGuard(guardianType: GuardianType.Production, guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.ProductionCreator, simulationType: _simulationType, _messageHub));
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
                _simulation.SimulationContext.Tell(message: Directory.Instruction.Default
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
            var maxBucketSize = configuration.GetOption<MaxBucketSize>().Value;
            var timeConstraintQueueLength = configuration.GetOption<TimeConstraintQueueLength>().Value;

            // Get All Resources that have an Agent (that are limited)
            //var resources = _dBContext.Resources.ToList(); // all Resources
            var limitedResources = _dBContext.Resources.Where(x => x.IsPhysical).ToList(); // all Limited Resources


            foreach (var resource in limitedResources)
            {
                var capabilityProviders = GetCapabilityProviders(resource);
                System.Diagnostics.Debug.WriteLine($"Creating Resource: {resource.Name} with {capabilityProviders.Count} capabilities");
                //TODO: !IMPORTANT Max bucket size dynamic by Setup Count? 

                var capabilityProviderDefinition = new FCapabilityProviderDefinition(workTimeGenerator: randomWorkTime
                    , resource: resource
                    , capabilityProvider: capabilityProviders
                    , maxBucketSize: maxBucketSize
                    , timeConstraintQueueLength: timeConstraintQueueLength
                    , debug: _debugAgents);
                        _simulation.SimulationContext
                    .Tell(message: Directory.Instruction.Default
                                            .CreateMachineAgents
                                            .Create(message: capabilityProviderDefinition, target: ActorPaths.HubDirectory.Ref)
                        , sender: ActorPaths.HubDirectory.Ref);
            }
        }

        public List<M_ResourceCapabilityProvider> GetCapabilityProviders(M_Resource resource)
        {
            var capabilityForResource = _dBContext.ResourceSetups
                .Include(x => x.ResourceCapabilityProvider)
                    .ThenInclude(x => x.ResourceCapability)
                .Where(x => x.ResourceId == resource.Id).ToList();

            var capabilityProviderIds = capabilityForResource.Select(x => x.ResourceCapabilityProviderId);
            var capabilitesForResource = _dBContext.ResourceCapabilityProviders
                                                   .Include(x => x.ResourceCapability)
                                                        .ThenInclude(x => x.ParentResourceCapability)
                                                   .Include(x => x.ResourceSetups)
                                                        .ThenInclude(x => x.Resource)
                                                            .Where(x => capabilityProviderIds.Contains(x.Id));
            
            return capabilitesForResource.ToList();
        }
    }
}

