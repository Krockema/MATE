using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Hive;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
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
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Central;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;
using Mate.Production.Core.Interfaces;
using Mate.Production.Core.Reporting;
using Mate.Production.Core.SignalR;

namespace Mate.Production.Core
{
    public class GanttSimulation : BaseSimulation, ISimulation
    {
        public DataBase<GanttPlanDBContext> dbGantt { get; }
        public string dbMate { get;}
        private Time StartTime = new Time(new DateTime(2000, 01, 01));
        private ResourceDictionary _resourceDictionary { get; }
        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="debug">Enables AKKA-Global message Debugging</param>
        public GanttSimulation(string dbName, IMessageHub messageHub) : base(dbName, messageHub)
        {
            dbGantt = Dbms.GetGanttDataBase(DataBaseConfiguration.GP);
            dbMate = dbName;
            _resourceDictionary = new ResourceDictionary();
        }
        public override Task<Hive> InitializeSimulation(Configuration configuration)
        {
            return Task.Run(function: () =>
            {
                MessageHub.SendToAllClients(msg: "Initializing Simulation...");
                // Init Simulation
                HiveConfig = configuration.GetContextConfiguration();
                DebugAgents = configuration.GetOption<DebugAgents>().Value;
                SimulationType = configuration.GetOption<SimulationKind>().Value;
                StartTime = new Time(configuration.GetOption<SimulationStartTime>().Value);
                Hive = new Hive(HiveConfig);
                StateManager = new GanttStateManager(new List<IActorRef> { this.StorageCollector
                                                                         , this.JobCollector
                                                                         , this.ContractCollector
                                                                         , this.ResourceCollector
                                                                     }
                                                        , MessageHub
                                                        , Hive);
                ActorPaths = new ActorPaths(simulationContext: Hive.ContextManagerRef);
                ActorPaths.SetStateManagerRef(HiveConfig.StateManagerRef);

                foreach (var worker in dbGantt.DbContext.GptblWorker)
                {
                    var workergroup = dbGantt.DbContext.GptblWorkergroupWorker.Single(x => x.WorkerId.Equals(worker.Id));

                    _resourceDictionary.Add(int.Parse(worker.Id), new ResourceDefinition(name: worker.Name,
                        id: int.Parse(worker.Id),
                        actorRef: ActorRefs.Nobody,
                        groupId: workergroup.WorkergroupId,
                        resourceType: ResourceType.Worker));
                }
                dbGantt.DbContext.GptblPrt.Where(x => !x.CapacityType.Equals(1)).Select(x => new { x.Id, x.Name}).ToList().ForEach(x => _resourceDictionary.Add(int.Parse(x.Id),new ResourceDefinition(
                    name: x.Name,
                    id: int.Parse(x.Id),
                    actorRef: ActorRefs.Nobody,
                    groupId: "1",
                    resourceType: ResourceType.Tool)));
                dbGantt.DbContext.GptblWorkcenter.Select(x => new { x.Id, x.Name }).ToList().ForEach(x => _resourceDictionary.Add(int.Parse(x.Id), new ResourceDefinition(
                    name: x.Name,
                    id: int.Parse(x.Id),
                    actorRef: ActorRefs.Nobody,
                    groupId: string.Empty,
                    resourceType: ResourceType.Workcenter)));

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



                return Hive;
            });
        }
        private void CreateCollectorAgents(Configuration configuration)
        {
            DateTime settlingStart = configuration.GetOption<SimulationStartTime>().Value + configuration.GetOption<SettlingStart>().Value;
            StorageCollector = Hive.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsStorage.Get()
                                                            , msgHub: MessageHub, configuration: configuration, time: StartTime, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticsStorage.GetStreamTypes()), name: "StorageCollector");
            ContractCollector = Hive.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsContracts.Get()
                                                            , msgHub: MessageHub, configuration: configuration, time: StartTime, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticsContracts.GetStreamTypes()), name: "ContractCollector");
            JobCollector = Hive.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticJob.Get(resources: _resourceDictionary, settlingStart: settlingStart)
                                                            , msgHub: MessageHub, configuration: configuration, time: StartTime, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticJob.GetStreamTypes()), name: "JobCollector");
            ResourceCollector = Hive.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticResource.Get(resources: _resourceDictionary, configuration.GetOption<SimulationStartTime>().Value)
                                                            , msgHub: MessageHub, configuration: configuration, time: StartTime, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticResource.GetStreamTypes()), name: "ResourceCollector");
        }
        private void CreateSupervisor(Configuration configuration)
        {
            var products = dbGantt.DbContext.GptblMaterial.Where(x => x.Info1 == "Product").ToList();
            var initialTime = configuration.GetOption<EstimatedThroughPut>().Value;

            var estimatedThroughPuts = products.Select(a => new SetEstimatedThroughputTimeRecord(int.Parse(a.MaterialId), initialTime, a.Name))
                .ToList();

            ActorPaths.SetSupervisorAgent(systemAgent: Hive.ActorSystem
                .ActorOf(props: Supervisor.Props(actorPaths: ActorPaths,
                        configuration: configuration,
                        time: StartTime,
                        hiveConfig: HiveConfig,
                        debug: DebugAgents,
                        principal: ActorRefs.Nobody),
                    name: "Supervisor"));

            var behave = Agents.SupervisorAgent.Behaviour.Factory.Central(
                dbNameGantt: dbGantt.DataBaseName.Value,
                dbNameProduction: dbMate,
                messageHub: MessageHub,
                hiveConfig: HiveConfig,
                configuration: configuration,
                estimatedThroughputTimes: estimatedThroughPuts);
            Hive.ContextManagerRef.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.SystemAgent.Ref, message: behave));
        }

        private void CreateDirectoryAgents(Configuration configuration)
        {
            // Resource Directory
            ActorPaths.SetHubDirectoryAgent(hubAgent: Hive.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, configuration: configuration, hiveConfig: HiveConfig, time: StartTime, debug: DebugAgents), name: "HubDirectory"));
            Hive.ContextManagerRef.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.HubDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: SimulationType)));
            CreateHubAgent(ActorPaths.HubDirectory.Ref, configuration);

            // Storage Directory
            ActorPaths.SetStorageDirectory(storageAgent: Hive.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, configuration: configuration, hiveConfig: HiveConfig, time: StartTime, debug: DebugAgents), name: "StorageDirectory"));
            Hive.ContextManagerRef.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.StorageDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: SimulationType)));
        }

        private void CreateHubAgent(IActorRef directory, Configuration configuration)
        {
            WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration: configuration, 0);

            var hubInfo = new Environment.Records.Central.ResourceHubInformationRecord(ResourceList: _resourceDictionary
                                              , DbConnectionString: dbGantt.ConnectionString.Value
                                         , MasterDbConnectionString: base.DbProduction.ConnectionString.Value
                                         , PathToGANTTPLANOptRunner: configuration.GetOption<GANTTPLANOptRunnerPath>().Value
                                               , WorkTimeGenerator: randomWorkTime);
            Hive.ContextManagerRef.Tell(
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
            var guard = Hive.ActorSystem.ActorOf(props: Guardian.Props(actorPaths: ActorPaths, configuration: configuration, hiveConfig: HiveConfig, time: StartTime, debug: DebugAgents), name: guardianType.ToString() + "Guard");
            Hive.ContextManagerRef.Tell(message: BasicInstruction.Initialize.Create(target: guard, message: guardianBehaviour));
            ActorPaths.AddGuardian(guardianType: guardianType, actorRef: guard);
        }

        /// <summary>
        /// Creates an Time Agent that is listening to Clock AdvanceTo messages and serves the frontend with current time updates
        /// </summary>
        private void AddTimeMonitor()
        {
            Action<Time> tm = (timePeriod) => MessageHub.SendToClient(listener: "clockListener", msg: timePeriod.Value.ToString());
            var timeMonitor = Props.Create(factory: () => new TimeMonitor((timePeriod) => tm(timePeriod)));
            Hive.ActorSystem.ActorOf(props: timeMonitor, name: "TimeMonitor");
        }

        private void AddDeadLetterMonitor()
        {
            var deadletterWatchMonitorProps = Props.Create(factory: () => new Reporting.DeadLetterMonitor());
            var deadletterWatchActorRef = Hive.ActorSystem.ActorOf(props: deadletterWatchMonitorProps, name: "DeadLetterMonitoringActor");
            //subscribe to the event stream for messages of type "DeadLetter"
            Hive.ActorSystem.EventStream.Subscribe(subscriber: deadletterWatchActorRef, channel: typeof(DeadLetter));
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

                var stockDefintion = new CentralStockDefinitionRecord(
                    StockId: Int32.Parse(material.MaterialId), 
                    MaterialName: material.Name, 
                    InitialQuantity: initialStock.Quantity.Value, 
                    Unit: material.QuantityUnitId, 
                    Price: material.ValueProduction.Value,
                    MaterialType:material.Info1, 
                    DeliveryPeriod: TimeSpan.FromMinutes(long.Parse(material.Info2))
                    );

                Hive.ContextManagerRef.Tell(message: Directory.Instruction.Central
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

                var resourceDefinition = new CentralResourceDefinitionRecord(ResourceId: resource.Key
                                                                            , ResourceName: resource.Value.Name
                                                                            , resource.Value.GroupId
                                                                            , (int)resource.Value.ResourceType);

                Hive.ContextManagerRef
                    .Tell(message: Directory.Instruction.Central
                                            .CreateMachineAgents
                                            .Create(message: resourceDefinition, target: ActorPaths.HubDirectory.Ref)
                        , sender: ActorPaths.HubDirectory.Ref);
            }
        }
    }
}

