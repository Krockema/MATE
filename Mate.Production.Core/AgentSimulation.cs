using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Hive;
using Akka.Hive.Action;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Mate.DataCore.Data.Initializer.StoredProcedures;
using Mate.DataCore.DataModel;
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
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;
using Mate.Production.Core.Interfaces;
using Mate.Production.Core.Reporting;
using Mate.Production.Core.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Mate.Production.Core
{
    public class AgentSimulation : BaseSimulation, ISimulation
    {
        private Time StartTime = new Time(new DateTime(2000, 01, 01));
            /// <summary>
            /// Prepare Simulation Environment
            /// </summary>
            /// <param name="debug">Enables AKKA-Global message Debugging</paSram>
        public AgentSimulation(string dbName, IMessageHub messageHub) : base(dbName, messageHub)
        {
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
                // Create DataCollectors
                CreateCollectorAgents(configuration: configuration);
                //Create Measurment Agent if required

                ActorPaths = new ActorPaths(simulationContext: Hive.ContextManagerRef);
                CreateMeasurementComponents(configuration: configuration);

                HiveConfig.StateManagerRef = Hive.ActorSystem.ActorOf(AgentStateManager.Props(new List<IActorRef> { this.StorageCollector
                                                                         , this.JobCollector
                                                                         , this.ContractCollector
                                                                         , this.ResourceCollector
                                                                         , this.MeasurementCollector
                                                                     }, Hive));

                ActorPaths.SetStateManagerRef(HiveConfig.StateManagerRef);
                //if (_debugAgents) 
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


                return Hive;
            });
        }

        private TimeSpan GetCustomOrderTime(int productId)
        {
            return ArticleStatistics.DeliveryDateEstimator(productId, 10, DbProduction.DbContext);
        } 


        private void CreateSupervisor(Configuration configuration)
        {
            var products = DbProduction.DbContext.GetProducts();
            // var initialTime = configuration.GetOption<EstimatedThroughPut>().Value;

            var estimatedThroughPuts = products.Select(a => new SetEstimatedThroughputTimeRecord(a.Id
                                , GetCustomOrderTime(a.Id)
                                , a.Name))
                                .ToList();




            var behave = Agents.SupervisorAgent.Behaviour.Factory.Default(
                                    dbNameProduction: DbProduction.DataBaseName.Value,
                                    messageHub: MessageHub,
                                    hiveConfig: HiveConfig,
                                    configuration: configuration,
                                    estimatedThroughputTimes: estimatedThroughPuts);
            ActorPaths.SetSupervisorAgent(systemAgent: Hive.ActorSystem
                .ActorOf(props: Supervisor.Props(actorPaths: ActorPaths,
                        hiveConfig: HiveConfig,
                        configuration: configuration,
                        time: StartTime,
                        debug: DebugAgents,
                        principal: ActorRefs.Nobody),
                    name: "Supervisor"));
            Hive.ContextManagerRef.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.SystemAgent.Ref, message: behave));
        }

        private void CreateDirectoryAgents(Configuration configuration)
        {
            // Resource Directory
            ActorPaths.SetHubDirectoryAgent(hubAgent: Hive.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, configuration: configuration, hiveConfig: HiveConfig, time: StartTime, debug: DebugAgents), name: "HubDirectory"));
            Hive.ContextManagerRef.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.HubDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: SimulationType)));
            CreateHubAgents(ActorPaths.HubDirectory.Ref, configuration);

            // Storage Directory
            ActorPaths.SetStorageDirectory(storageAgent: Hive.ActorSystem.ActorOf(props: Directory.Props(actorPaths: ActorPaths, configuration: configuration, hiveConfig: HiveConfig, time: StartTime, debug: DebugAgents), name: "StorageDirectory"));
            Hive.ContextManagerRef.Tell(message: BasicInstruction.Initialize.Create(target: ActorPaths.StorageDirectory.Ref, message: Agents.DirectoryAgent.Behaviour.Factory.Get(simType: SimulationType)));
        }

        private void CreateHubAgents(IActorRef directory, Configuration configuration)
        {
           
            var capabilities = DbProduction.DbContext.ResourceCapabilities.Where(x => x.ParentResourceCapabilityId == null).ToList();
            for (var index = 0; index < capabilities.Count; index++)
            {
                WorkTimeGenerator randomWorkTime = WorkTimeGenerator.Create(configuration: configuration, index);
                var capability = capabilities[index];
                GetCapabilitiesRecursive(capability);

                var hubInfo = new ResourceHubInformationRecord(Capability: capability
                                                                           ,WorkTimeGenerator: randomWorkTime
                                                                              ,MaxBucketSize: TimeSpan.FromMinutes(configuration.GetOption<MaxBucketSize>().Value));
                Hive.ContextManagerRef.Tell(
                    message: Directory.Instruction.Default.CreateResourceHubAgents.Create(hubInfo, directory),
                    sender: ActorRefs.NoSender);
            }
        }

        private void GetCapabilitiesRecursive(M_ResourceCapability capability)
        {
            capability.ChildResourceCapabilities = DbProduction.DbContext.ResourceCapabilities.Where(x => x.ParentResourceCapabilityId == capability.Id).ToList();
            capability.ChildResourceCapabilities.ToList().ForEach(GetCapabilitiesRecursive);
        }

        private void CreateCollectorAgents(Configuration configuration)
        {
            DateTime settlingStart = configuration.GetOption<SimulationStartTime>().Value + configuration.GetOption<SettlingStart>().Value;

            var resourcelist = new ResourceDictionary(); 
            DbProduction.DbContext.Resources.Where(x => x.IsPhysical)
                                .Select(selector: x => new {x.Id, Name = x.Name.Replace(" ", ""), x.IsBiological }).ToList()
                                .ForEach(x => resourcelist.Add(x.Id, new ResourceDefinition(x.Name, x.Id, ActorRefs.Nobody, string.Empty, x.IsBiological? ResourceType.Worker : ResourceType.Workcenter)));

            StorageCollector = Hive.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsStorage.Get()
                                                            , msgHub: MessageHub, configuration: configuration, time: StartTime, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticsStorage.GetStreamTypes()), name: "StorageCollector");
            ContractCollector = Hive.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsContracts.Get()
                                                            , msgHub: MessageHub, configuration: configuration, time: StartTime, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticsContracts.GetStreamTypes()), name: "ContractCollector");
            JobCollector = Hive.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticJob.Get(resources: resourcelist, settlingStart)
                                                            , msgHub: MessageHub, configuration: configuration, time: StartTime, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticJob.GetStreamTypes()), name: "JobCollector");
            ResourceCollector = Hive.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticResource.Get(resources: resourcelist, configuration.GetOption<SimulationStartTime>().Value)
                                                            , msgHub: MessageHub, configuration: configuration, time: StartTime, debug: DebugAgents
                                                            , streamTypes: CollectorAnalyticResource.GetStreamTypes()), name: "ResourceCollector");
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
            var guard = Hive.ActorSystem.ActorOf(props: Guardian.Props(actorPaths: ActorPaths, configuration: configuration, hiveConfig: HiveConfig,time: StartTime, debug: DebugAgents), name: guardianType.ToString() + "Guard");
            Hive.ContextManagerRef.Tell(message: BasicInstruction.Initialize.Create(target: guard, message: guardianBehaviour));
            ActorPaths.AddGuardian(guardianType: guardianType, actorRef: guard);
        }

        /// <summary>
        /// Creates an Time Agent that is listening to Clock AdvanceTo messages and serves the frontend with current time updates
        /// </summary>
        private void AddTimeMonitor()
        {
            Action<Time> tm = (timePeriod) => MessageHub.SendToClient(listener: "clockListener", msg: timePeriod.ToString());
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
            foreach (var stock in DbProduction.DbContext.Stocks
                                            .Include(navigationPropertyPath: x => x.StockExchanges)
                                            .Include(navigationPropertyPath: x => x.Article).ThenInclude(navigationPropertyPath: x => x.ArticleToBusinessPartners)
                                                                    .ThenInclude(navigationPropertyPath: x => x.BusinessPartner)
                                            .Include(navigationPropertyPath: x => x.Article).ThenInclude(navigationPropertyPath: x => x.ArticleType)
                                            .AsNoTracking().ToList())
            {
                Hive.ContextManagerRef.Tell(message: Directory.Instruction.Default
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
            var maxBucketSize = TimeSpan.FromMinutes(configuration.GetOption<MaxBucketSize>().Value);
            var timeConstraintQueueLength = configuration.GetOption<TimeConstraintQueueLength>().Value;

            // Get All Resources that have an Agent (that are limited)
            //var resources = _dBContext.Resources.ToList(); // all Resources
            var limitedResources = DbProduction.DbContext.Resources
                                                    .Include(x => x.ResourceSetups)
                                                    .Where(x => x.IsPhysical).ToList(); // all Limited Resources

            foreach (var resource in limitedResources)
            {
                var capabilityProviders = GetCapabilityProviders(resource);
                System.Diagnostics.Debug.WriteLine($"Creating Resource: {resource.Name} with {capabilityProviders.Count} capabilities");
                //TODO: !IMPORTANT Max bucket size dynamic by Setup Count? 
                var processing = resource.ResourceSetups.Any(x => x.UsedInProcess);
                var setup = resource.ResourceSetups.Any(x => x.UsedInSetup);
                ResourceType resourceType = ResourceType.Tool;
                switch ((setup, processing))
                {
                    case (true, false):
                        resourceType = ResourceType.Operator;
                        break;
                    case (false, true):
                        resourceType = ResourceType.Worker;
                        break;
                    case (true, true):
                        resourceType = ResourceType.Workcenter;
                        break;
                }

                var capabilityProviderDefinition = new CapabilityProviderDefinitionRecord(WorkTimeGenerator: randomWorkTime
                    , Resource: resource
                    , ResourceType: resourceType
                    , CapabilityProvider: capabilityProviders
                    , MaxBucketSize: maxBucketSize
                    , TimeConstraintQueueLength: timeConstraintQueueLength
                    , Debug: DebugAgents);
                        Hive.ContextManagerRef
                    .Tell(message: Directory.Instruction.Default
                                            .CreateMachineAgents
                                            .Create(message: capabilityProviderDefinition, target: ActorPaths.HubDirectory.Ref)
                        , sender: ActorPaths.HubDirectory.Ref);
            }
        }

        public List<M_ResourceCapabilityProvider> GetCapabilityProviders(M_Resource resource)
        {
            var capabilityForResource = DbProduction.DbContext.ResourceSetups
                .Include(x => x.ResourceCapabilityProvider)
                    .ThenInclude(x => x.ResourceCapability)
                .Where(x => x.ResourceId == resource.Id).ToList();

            var capabilityProviderIds = capabilityForResource.Select(x => x.ResourceCapabilityProviderId);
            var capabilitesForResource = DbProduction.DbContext.ResourceCapabilityProviders
                                                   .Include(x => x.ResourceCapability)
                                                        .ThenInclude(x => x.ParentResourceCapability)
                                                   .Include(x => x.ResourceSetups)
                                                        .ThenInclude(x => x.Resource)
                                                            .Where(x => capabilityProviderIds.Contains(x.Id));
            
            return capabilitesForResource.ToList();
        }

        private void CreateMeasurementComponents(Configuration configuration)
        {
            ActorPaths.SetMeasurementAgent(measurementActorRef: Hive.ActorSystem
                .ActorOf(props: Agents.ResourceAgent.Resource.Props(
                        actorPaths: ActorPaths,
                        configuration: configuration,
                        hiveConfig: HiveConfig,
                        resource: null,
                        time: StartTime,
                        debug: DebugAgents,
                        principal: ActorRefs.Nobody,
                        measurementActorRef: ActorRefs.Nobody),
                    name: "Measure"));

            Hive.ContextManagerRef.Tell(message:
                BasicInstruction.Initialize.Create(target: ActorPaths.MeasurementAgent.Ref,
                    message: Agents.ResourceAgent.Behaviour.Measurement.Get(configuration.GetOption<Environment.Options.Seed>())));

            MeasurementCollector = Hive.ActorSystem.ActorOf(props: Collector.Props(actorPaths: ActorPaths, collectorBehaviour: CollectorAnalyticsMeasurements.Get()
                , msgHub: MessageHub, configuration: configuration, time: StartTime, debug: DebugAgents
                , streamTypes: CollectorAnalyticsMeasurements.GetStreamTypes()), name: "MeasurementCollector");
        }
    }
}

