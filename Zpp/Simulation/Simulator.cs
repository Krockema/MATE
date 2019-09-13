using Akka.Actor;
using AkkaSim.Definitions;
using System.Diagnostics;
using NLog.Targets;
using Zpp.DbCache;
using Zpp.Simulation.Agents.JobDistributor;
using Zpp.Simulation.Agents.JobDistributor.Skills;
using Zpp.Simulation.Agents.JobDistributor.Types;
using Zpp.Simulation.Monitors;
using Zpp.Simulation.Types;
using Zpp.WrappersForPrimitives;

namespace Zpp.Simulation
{
    public class Simulator
    {
        private readonly IDbMasterDataCache _dbMasterDataCache;
        private readonly IDbTransactionData _dbTransactionData;
        private long _currentTime { get; set; } = 0;
        private SimulationConfig _simulationConfig { get; }
        private AkkaSim.Simulation _akkaSimulation { get; set; }
        public SimulationInterval _simulationInterval { get; private set; }

        public Simulator(IDbMasterDataCache dbMasterDataCache, IDbTransactionData dbTransactionData)
        {
            _simulationConfig = new SimulationConfig(false, 300);
            _dbTransactionData = dbTransactionData;
            _dbMasterDataCache = dbMasterDataCache;
        }


        public bool ProcessCurrentInterval(SimulationInterval simulationInterval)
        {
            Debug.WriteLine("Start simulation system. . . ");
            _simulationInterval = simulationInterval;

            _currentTime = simulationInterval.StartAt;
            _akkaSimulation = new AkkaSim.Simulation(_simulationConfig);
            var jobDistributor = _akkaSimulation.ActorSystem
                                                .ActorOf(props: JobDistributor.Props(_akkaSimulation.SimulationContext, _currentTime)
                                                        , name: "JobDistributor");

            // ToDo reflect CurrentTimespawn ?
            _akkaSimulation.Shutdown(simulationInterval.EndAt);
            // Create a Machines
            CreateResource(jobDistributor);
            
            // Set purchased Demands finished.
            ProvideRequiredPurchaseForThisInterval(simulationInterval);
            
            // Distribute Ready Jobs
            ProvideJobDistributor(jobDistributor);

            // TODO What to do with finished Jobs? How is PrO connected to the StockExchange. 
            /// a. Provide a Stockexchange Key with every ProductionOrder to complete SE.
            /// b. delete all Stockexchanges that are "ToProduce"
            /// --> _c. satisfy the first not yet satisfied Stockexchange 
            // Handle JobFinish

            var monitor = _akkaSimulation.ActorSystem
                                         .ActorOf(props: WorkTimeMonitor.Props(time: _currentTime),
                                                   name: "SimulationMonitor");
            if(_akkaSimulation.IsReady())
            {
                _akkaSimulation.RunAsync();
                Continuation(_simulationConfig.Inbox, _akkaSimulation);
            }



            Debug.WriteLine("System shutdown. . . ");
            Debug.WriteLine("System Runtime " + _akkaSimulation.ActorSystem.Uptime);
            return true;
        }

        private void ProvideJobDistributor(IActorRef jobDistributor)
        {
            var operationManager = new OperationManager(_dbMasterDataCache, _dbTransactionData);
            _akkaSimulation.SimulationContext
                           .Tell(message: OperationsToDistribute.Create(operationManager, jobDistributor)
                                ,sender: ActorRefs.NoSender);
        }

        /// <summary>
        /// can be done by some sort of StockManager later if time periods not static
        /// </summary>
        /// <param name="simulationInterval"></param>
        private void ProvideRequiredPurchaseForThisInterval(SimulationInterval simulationInterval)
        {
            var from = new DueTime((int)simulationInterval.StartAt);
            var to = new DueTime((int)simulationInterval.EndAt);
            var stockExchanges = _dbTransactionData.GetAggregator().GetProvidersForInterval(from, to);
            foreach (var stockExchange in stockExchanges)
            {
                stockExchange.SetProvided(stockExchange.GetDueTime());
            }
        }

        private void CreateResource(IActorRef jobDistributor)
        {
            var machines = ResourceManager.GetResources(_dbMasterDataCache);
            var createMachines = AddResources.Create(machines, jobDistributor);
            _akkaSimulation.SimulationContext.Tell(createMachines, ActorRefs.Nobody);
        }

        // TODO: replace --> with Simulator.Continuation(Inbox inbox, AkkaSim.Simulation sim);
        private static void Continuation(Inbox inbox, AkkaSim.Simulation sim)
        {
            var something = inbox.ReceiveAsync().Result;
            switch (something)
            {
                case SimulationMessage.SimulationState.Started:
                    Debug.WriteLine($"Simulation Start", "AKKA");
                    Continuation(inbox, sim);
                    break;
                case SimulationMessage.SimulationState.Stopped:
                    Debug.WriteLine($"Simulation Stop.", "AKKA");
                    sim.Continue();
                    Continuation(inbox, sim);
                    break;
                case SimulationMessage.SimulationState.Finished:
                    Debug.WriteLine($"Simulation Finish.", "AKKA");
                    sim.ActorSystem.Terminate();
                    break;
                default:
                    break;
            }
        }
    }
}
