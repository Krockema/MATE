using Akka.Actor;
using Akka.Event;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using Master40.MessageSystem.SignalR;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Reporting;
using Master40.SimulationImmutables;
using Master40.Tools.Simulation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;


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
            return Task.Run(async () =>
            {
                Statistics.Log = new List<string>();
                OrderGenerator.GenerateOrdersSyncron(_DBContext, simConfig, 1); // .RunSynchronously();
                _messageHub.SendToAllClients("Initializing Simulation...");
                var randomWorkTime = new WorkTimeGenerator(simConfig.Seed, simConfig.WorkTimeDeviation, 0);

               // #1 Init Simulation
                _simulation = new AkkaSim.Simulation(contextConfig);
                ActorPaths = new ActorPaths(_simulation.SimulationContext, contextConfig.Inbox.Receiver);

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
                var deadletterWatchMonitorProps = Props.Create(() => new DeadLetterMonitor());
                var deadletterWatchActorRef = _simulation.ActorSystem.ActorOf(deadletterWatchMonitorProps, "DeadLetterMonitoringActor");

                // subscribe to the event stream for messages of type "DeadLetter"
                _simulation.ActorSystem.EventStream.Subscribe(deadletterWatchActorRef, typeof(DeadLetter));

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
                                                                        .ThenInclude(x => x.BusinessPartner).AsNoTracking())
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

        public static void Continuation(Inbox inbox, AkkaSim.Simulation sim)
        {

            var something = inbox.ReceiveAsync(System.TimeSpan.FromHours(1)).Result;
            switch (something)
            {
                case SimulationMessage.SimulationState.Started:
                    System.Diagnostics.Debug.WriteLine("AKKA:START AGENT SYSTEM", "AKKA-System:");
                    Continuation(inbox, sim);
                    break;
                case SimulationMessage.SimulationState.Stopped:
                    System.Diagnostics.Debug.WriteLine("AKKA:STOP AGENT SYSTEM", "AKKA-System:");
                    sim.Continue();
                    Continuation(inbox, sim);
                    break;
                case SimulationMessage.SimulationState.Finished:
                    System.Diagnostics.Debug.WriteLine("SHUTDOWN AGENT SYSTEM", "AKKA-System:");
                    sim.ActorSystem.Terminate();
                    break;
                default:
                    break;
            }
        }

    }
}
