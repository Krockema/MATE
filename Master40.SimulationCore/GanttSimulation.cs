using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using AkkaSim;
using AkkaSim.Definitions;
using AkkaSim.SpecialActors;
using Master40.DB.Data.Context;
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
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.HubAgent.Types.Central;
using static FCentralResourceHubInformations;
using static FCentralStockDefinitions;
using static FSetEstimatedThroughputTimes;

namespace Master40.SimulationCore
{
    public class GanttSimulation
    {
        // public ActorSystem ActorSystem { get; }
        // public IActorRef SimulationContext { get; }
        private GanttPlanDBContext _ganttContext { get; }
        private ProductionDomainContext _productionContext { get; }
        private ResourceDictionary _resourceDictionary { get; }
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
        public IActorRef HubAgent { get; private set; }
        public GanttStateManager StateManager { get; private set; }
        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="debug">Enables AKKA-Global message Debugging</param>
        public GanttSimulation(string ganttContextDbString, string productionContextDbString, IMessageHub messageHub)
        {
            _ganttContext = GanttPlanDBContext.GetContext(ganttContextDbString);
            _productionContext = ProductionDomainContext.GetContext(productionContextDbString);
            _messageHub = messageHub;
            _resourceDictionary = new ResourceDictionary();
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
                ActorPaths = new ActorPaths(simulationContext: _simulation.SimulationContext
                                              , systemMailBox: SimulationConfig.Inbox.Receiver);

                foreach (var worker in _ganttContext.GptblWorker)
                {
                    var workergroup = _ganttContext.GptblWorkergroupWorker.Single(x => x.WorkerId.Equals(worker.Id));

                    _resourceDictionary.Add(worker.Id, new ResourceDefinition(worker.Name, worker.Id, ActorRefs.Nobody, workergroup.WorkergroupId, resourceType: 3));
                }
                _ganttContext.GptblPrt.Where(x => !x.CapacityType.Equals(1)).Select(x => new { x.Id, x.Name}).ForEach(x => _resourceDictionary.Add(x.Id,new ResourceDefinition(x.Name, x.Id, ActorRefs.Nobody,"1", resourceType: 5)));
                _ganttContext.GptblWorkcenter.Select(x => new { x.Id, x.Name }).ForEach(x => _resourceDictionary.Add(x.Id, new ResourceDefinition(x.Name, x.Id, ActorRefs.Nobody, string.Empty, resourceType: 1)));

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
                StateManager = new GanttStateManager(new List<IActorRef> { this.StorageCollector
                                                                         , this.JobCollector
                                                                         , this.ContractCollector
                                                                         , this.ResourceCollector
                                                                     }
                                                        , SimulationConfig.Inbox);

                return _simulation;
            });
        }
        private void CreateCollectorAgents(Configuration configuration)
        {
            StorageCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsStorage.Get()
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticsStorage.GetStreamTypes()), name: "StorageCollector");
            ContractCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsContracts.Get()
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticsContracts.GetStreamTypes()), name: "ContractCollector");
            JobCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticJob.Get(resources: _resourceDictionary)
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticJob.GetStreamTypes()), name: "JobCollector");
            ResourceCollector = _simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticResource.Get(resources: _resourceDictionary)
                                                            , msgHub: _messageHub, configuration: configuration, time: 0, debug: _debugAgents
                                                            , streamTypes: CollectorAnalyticResource.GetStreamTypes()), name: "ResourceCollector");
        }
        private void CreateSupervisor(Configuration configuration)
        {
            var products = _ganttContext.GptblMaterial.Where(x => x.Info1 == "Product").ToList();
            var initialTime = configuration.GetOption<EstimatedThroughPut>().Value;

            var estimatedThroughPuts = products.Select(a => new FSetEstimatedThroughputTime(int.Parse(a.MaterialId), initialTime, a.Name))
                .ToList();

            ActorPaths.SetSupervisorAgent(systemAgent: _simulation.ActorSystem
                .ActorOf(props: Supervisor.Props(actorPaths: ActorPaths,
                        time: 0,
                        debug: _debugAgents,
                        principal: ActorRefs.Nobody),
                    name: "Supervisor"));

            var behave = Agents.SupervisorAgent.Behaviour.Factory.Central(
                ganttContext: _ganttContext,
                productionDomainContext: _productionContext,
                messageHub: _messageHub,
                configuration: configuration,
                estimatedThroughputTimes: estimatedThroughPuts);
            _simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.SystemAgent.Ref, message: behave));
        }

        private void CreateDirectoryAgents(Configuration configuration)
        {
            // Resource Directory
            ActorPaths.SetHubDirectoryAgent(hubAgent: _simulation.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, time: 0, debug: _debugAgents), name: "HubDirectory"));
            _simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.HubDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: _simulationType)));
            CreateHubAgent(ActorPaths.HubDirectory.Ref, configuration);

            // Storage Directory
            ActorPaths.SetStorageDirectory(storageAgent: _simulation.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, time: 0, debug: _debugAgents), name: "StorageDirectory"));
            _simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.StorageDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: _simulationType)));
        }

        private void CreateHubAgent(IActorRef directory, Configuration configuration)
        {
            WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration: configuration, 0);

            var hubInfo = new FResourceHubInformation(resourceList: _resourceDictionary
                                              , dbConnectionString: _ganttContext.Database.GetDbConnection().ConnectionString
                                         ,masterDbConnectionString: _productionContext.Database.GetDbConnection().ConnectionString
                                               , workTimeGenerator: randomWorkTime);
            _simulation.SimulationContext.Tell(
                message: Directory.Instruction.Central.CreateHubAgent.Create(hubInfo, directory),
                sender: ActorRefs.NoSender);
            
        }

        private void GenerateGuardians()
        {
            CreateGuard(guardianType: GuardianType.Contract, guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.ContractCreator, simulationType: _simulationType, _messageHub));
            CreateGuard(guardianType: GuardianType.Dispo, guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.DispoCreator, simulationType: _simulationType, _messageHub));
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
            var materials = _ganttContext.GptblMaterial;

            var stockpostings = _ganttContext.GptblStockquantityposting;

            foreach (var material in materials)
            {
                var initialStock = stockpostings.Single(x => x.MaterialId.Equals(material.MaterialId));

                var stockDefintion = new FCentralStockDefinition(
                    stockId: Int32.Parse(material.MaterialId), 
                    materialName: material.Name, 
                    initialQuantity: initialStock.Quantity.Value, 
                    unit: material.QuantityUnitId, 
                    price: material.ValueProduction.Value,
                    materialType:material.Info1, 
                    deliveryPeriod: long.Parse(material.Info2)
                    );

                _simulation.SimulationContext.Tell(message: Directory.Instruction.Central
                                                            .CreateStorageAgents
                                                            .Create(message: stockDefintion, target: ActorPaths.StorageDirectory.Ref)
                                                        , sender: ActorPaths.StorageDirectory.Ref);

                System.Diagnostics.Debug.WriteLine($"Creating Stock for: {material.Name}");
            }
        }

        /// <summary>
        /// Creates ResourcesAgents based on current Model and add them to the SimulationContext
        /// </summary>
        /// <param name="configuration">Environment.Configuration</param>
        private void CreateResourceAgents(Configuration configuration)
        {
            WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration: configuration);


            foreach (var resource in _resourceDictionary)
            {
                System.Diagnostics.Debug.WriteLine($"Creating Resource: {resource.Value}");

                var resourceDefinition = new FCentralResourceDefinitions.FCentralResourceDefinition(resourceId: resource.Key, resourceName: resource.Value.Name, resource.Value.GroupId, resource.Value.ResourceType);

                _simulation.SimulationContext
                    .Tell(message: Directory.Instruction.Central
                                            .CreateMachineAgents
                                            .Create(message: resourceDefinition, target: ActorPaths.HubDirectory.Ref)
                        , sender: ActorPaths.HubDirectory.Ref);
            }
        }
    }
}

