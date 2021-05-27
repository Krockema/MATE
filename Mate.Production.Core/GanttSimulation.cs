using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using AkkaSim;
using AkkaSim.SpecialActors;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Nominal.Model;
using Mate.Production.Core.Agents;
using Mate.Production.Core.Agents.CollectorAgent;
using Mate.Production.Core.Agents.CollectorAgent.Types;
using Mate.Production.Core.Agents.DirectoryAgent;
using Mate.Production.Core.Agents.Guardian;
using Mate.Production.Core.Agents.Guardian.Options;
using Mate.Production.Core.Agents.HubAgent.Types.Central;
using Mate.Production.Core.Agents.SupervisorAgent;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;
using Mate.Production.Core.Interfaces;
using Mate.Production.Core.Reporting;
using Mate.Production.Core.SignalR;
using static FCentralResourceHubInformations;
using static FCentralStockDefinitions;
using static FSetEstimatedThroughputTimes;

namespace Mate.Production.Core
{
    public class GanttSimulation : BaseSimulation, ISimulation
    {
        public DataBase<GanttPlanDBContext> dbGantt { get; }
        private ResourceDictionary _resourceDictionary { get; }
        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="debug">Enables AKKA-Global message Debugging</param>
        public GanttSimulation(string dbName, IMessageHub messageHub) : base(dbName, messageHub)
        {
            dbGantt = Dbms.GetGanttDataBase("DBGP");
            _resourceDictionary = new ResourceDictionary();
        }
        public override Task<Simulation> InitializeSimulation(Configuration configuration)
        {
            return Task.Run(function: () =>
            {
                MessageHub.SendToAllClients(msg: "Initializing Simulation...");
                // Init Simulation
                SimulationConfig = configuration.GetContextConfiguration();
                DebugAgents = configuration.GetOption<DebugAgents>().Value;
                SimulationType = configuration.GetOption<SimulationKind>().Value;
                Simulation = new Simulation(simConfig: SimulationConfig);
                ActorPaths = new ActorPaths(simulationContext: Simulation.SimulationContext
                                              , systemMailBox: SimulationConfig.Inbox.Receiver);

                foreach (var worker in dbGantt.DbContext.GptblWorker)
                {
                    var workergroup = dbGantt.DbContext.GptblWorkergroupWorker.Single(x => x.WorkerId.Equals(worker.Id));

                    _resourceDictionary.Add(int.Parse(worker.Id), new ResourceDefinition(worker.Name, int.Parse(worker.Id), ActorRefs.Nobody, workergroup.WorkergroupId, resourceType: ResourceType.Worker));
                }
                dbGantt.DbContext.GptblPrt.Where(x => !x.CapacityType.Equals(1)).Select(x => new { x.Id, x.Name}).ToList().ForEach(x => _resourceDictionary.Add(int.Parse(x.Id),new ResourceDefinition(x.Name, int.Parse(x.Id), ActorRefs.Nobody,"1", resourceType: ResourceType.Tool)));
                dbGantt.DbContext.GptblWorkcenter.Select(x => new { x.Id, x.Name }).ToList().ForEach(x => _resourceDictionary.Add(int.Parse(x.Id), new ResourceDefinition(x.Name, int.Parse(x.Id), ActorRefs.Nobody, string.Empty, resourceType: ResourceType.Workcenter)));

            // Create DataCollectors
            CreateCollectorAgents(configuration: configuration);
                //if (DebugAgents) 
                AddDeadLetterMonitor();
                AddTimeMonitor();

                // Create Guardians and Inject Childcreators
                GenerateGuardians(configuration: configuration);

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
                                                        , MessageHub
                                                        , SimulationConfig.Inbox);

                return Simulation;
            });
        }
        private void CreateCollectorAgents(Configuration configuration)
        {
            StorageCollector = Simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsStorage.Get()
                                                            , msgHub: MessageHub, configuration: configuration, time: 0, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticsStorage.GetStreamTypes()), name: "StorageCollector");
            ContractCollector = Simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsContracts.Get()
                                                            , msgHub: MessageHub, configuration: configuration, time: 0, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticsContracts.GetStreamTypes()), name: "ContractCollector");
            JobCollector = Simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticJob.Get(resources: _resourceDictionary)
                                                            , msgHub: MessageHub, configuration: configuration, time: 0, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticJob.GetStreamTypes()), name: "JobCollector");
            ResourceCollector = Simulation.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticResource.Get(resources: _resourceDictionary)
                                                            , msgHub: MessageHub, configuration: configuration, time: 0, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticResource.GetStreamTypes()), name: "ResourceCollector");
        }
        private void CreateSupervisor(Configuration configuration)
        {
            var products = dbGantt.DbContext.GptblMaterial.Where(x => x.Info1 == "Product").ToList();
            var initialTime = configuration.GetOption<EstimatedThroughPut>().Value;

            var estimatedThroughPuts = products.Select(a => new FSetEstimatedThroughputTime(int.Parse(a.MaterialId), initialTime, a.Name))
                .ToList();

            ActorPaths.SetSupervisorAgent(systemAgent: Simulation.ActorSystem
                .ActorOf(props: Supervisor.Props(actorPaths: ActorPaths,
                        configuration: configuration,
                        time: 0,
                        debug: DebugAgents,
                        principal: ActorRefs.Nobody),
                    name: "Supervisor"));

            var behave = Agents.SupervisorAgent.Behaviour.Factory.Central(
                dbNameGantt: dbGantt.DataBaseName.Value,
                dbNameProduction: base.DbProduction.DataBaseName.Value,
                messageHub: MessageHub,
                configuration: configuration,
                estimatedThroughputTimes: estimatedThroughPuts);
            Simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.SystemAgent.Ref, message: behave));
        }

        private void CreateDirectoryAgents(Configuration configuration)
        {
            // Resource Directory
            ActorPaths.SetHubDirectoryAgent(hubAgent: Simulation.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, configuration: configuration, time: 0, debug: DebugAgents), name: "HubDirectory"));
            Simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.HubDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: SimulationType)));
            CreateHubAgent(ActorPaths.HubDirectory.Ref, configuration);

            // Storage Directory
            ActorPaths.SetStorageDirectory(storageAgent: Simulation.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, configuration: configuration, time: 0, debug: DebugAgents), name: "StorageDirectory"));
            Simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.StorageDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: SimulationType)));
        }

        private void CreateHubAgent(IActorRef directory, Configuration configuration)
        {
            WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration: configuration, 0);

            var hubInfo = new FResourceHubInformation(resourceList: _resourceDictionary
                                              , dbConnectionString: dbGantt.ConnectionString.Value
                                         , masterDbConnectionString: base.DbProduction.ConnectionString.Value
                                               , workTimeGenerator: randomWorkTime);
            Simulation.SimulationContext.Tell(
                message: Directory.Instruction.Central.CreateHubAgent.Create(hubInfo, directory),
                sender: ActorRefs.NoSender);
            
        }

        private void GenerateGuardians(Configuration configuration)
        {
            CreateGuard(guardianType: GuardianType.Contract,
                        guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.ContractCreator, simulationType: SimulationType, MessageHub),
                        configuration: configuration);
            CreateGuard(guardianType: GuardianType.Dispo, 
                        guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.DispoCreator, simulationType: SimulationType, MessageHub),
                        configuration: configuration);
            CreateGuard(guardianType: GuardianType.Production,
                        guardianBehaviour: GuardianBehaviour.Get(childMaker: ChildMaker.ProductionCreator, simulationType: SimulationType, MessageHub),
                        configuration: configuration);
        }

        private void CreateGuard(GuardianType guardianType, GuardianBehaviour guardianBehaviour, Configuration configuration)
        {
            var guard = Simulation.ActorSystem.ActorOf(props: Guardian.Props(actorPaths: ActorPaths, configuration: configuration,  time: 0, debug: DebugAgents), name: guardianType.ToString() + "Guard");
            Simulation.SimulationContext.Tell(message: BasicInstruction.Initialize.Create(target: guard, message: guardianBehaviour));
            ActorPaths.AddGuardian(guardianType: guardianType, actorRef: guard);
        }

        /// <summary>
        /// Creates an Time Agent that is listening to Clock AdvanceTo messages and serves the frontend with current time updates
        /// </summary>
        private void AddTimeMonitor()
        {
            Action<long> tm = (timePeriod) => MessageHub.SendToClient(listener: "clockListener", msg: timePeriod.ToString());
            var timeMonitor = Props.Create(factory: () => new TimeMonitor((timePeriod) => tm(timePeriod)));
            Simulation.ActorSystem.ActorOf(props: timeMonitor, name: "TimeMonitor");
        }

        private void AddDeadLetterMonitor()
        {
            var deadletterWatchMonitorProps = Props.Create(factory: () => new DeadLetterMonitor());
            var deadletterWatchActorRef = Simulation.ActorSystem.ActorOf(props: deadletterWatchMonitorProps, name: "DeadLetterMonitoringActor");
            //subscribe to the event stream for messages of type "DeadLetter"
            Simulation.ActorSystem.EventStream.Subscribe(subscriber: deadletterWatchActorRef, channel: typeof(DeadLetter));
        }

        /// <summary>
        /// Creates StorageAgents based on current Model and add them to the SimulationContext
        /// </summary>
        private void CreateStorageAgents()
        {
            var materials = dbGantt.DbContext.GptblMaterial;

            var stockpostings = dbGantt.DbContext.GptblStockquantityposting;

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

                Simulation.SimulationContext.Tell(message: Directory.Instruction.Central
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
                System.Diagnostics.Debug.WriteLine($"Creating Resource: {resource.Value.Name}");

                var resourceDefinition = new FCentralResourceDefinitions.FCentralResourceDefinition(resourceId: resource.Key, resourceName: resource.Value.Name, resource.Value.GroupId, (int)resource.Value.ResourceType);

                Simulation.SimulationContext
                    .Tell(message: Directory.Instruction.Central
                                            .CreateMachineAgents
                                            .Create(message: resourceDefinition, target: ActorPaths.HubDirectory.Ref)
                        , sender: ActorPaths.HubDirectory.Ref);
            }
        }
    }
}

