using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Dsl;
using AkkaSim;
using AkkaSim.Logging;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.HubAgent.Behaviour;
using Master40.SimulationCore.Agents.JobAgent;
using Master40.Tools.Connectoren.Ganttplan;
using NLog;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore
{
    public class GanttStateManager : StateManager
    {
        private readonly List<IActorRef> _collectorRefs;
        private readonly Inbox _inbox;
        private readonly Logger _logger;

        public GanttStateManager(List<IActorRef> collectorRefs, Inbox inbox)
        {
            _collectorRefs = collectorRefs;
            _inbox = inbox;
            _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
        }

        public void ContinueExecution(Simulation sim)
        {
            Continuation(_inbox, sim);
        }

        public override void AfterSimulationStarted(Simulation sim)
        {
            //base.AfterSimulationStarted(sim);
        }

        public override void AfterSimulationStopped(Simulation sim)
        {
            var tasks = new List<Task>();
            foreach (var item in _collectorRefs)
            {
                var msg = UpdateLiveFeed.Create(setup: false, target: _inbox.Receiver);
                _logger.Log(LogLevel.Info, $"Ask for Update Feed {item.Path.Name}");
                tasks.Add(item.Ask(message: msg, timeout: TimeSpan.FromSeconds(value: 60 * 60)));
                        
            }

            var hubActorRef = sim.ActorSystem.ActorSelection("/user/HubDirectory/CentralHub").ResolveOne(TimeSpan.FromSeconds(1)).Result;
            var instruction = Hub.Instruction.Central.LoadProductionOrders.Create(_inbox.Receiver, hubActorRef);
            sim.SimulationContext.Tell(instruction);

            Task.WaitAll(tasks.ToArray());
            var test = _inbox.ReceiveWhere(x => x.ToString() == "GanttPlan finished!", TimeSpan.FromSeconds(60));
        }

        public override void SimulationIsTerminating(Simulation sim)
        {
            foreach (var item in _collectorRefs)
            {
                item.Ask(message: UpdateLiveFeed.Create(setup: true, target: _inbox.Receiver)
                       , timeout: TimeSpan.FromHours(value: 1)).Wait();
            }
            sim.ActorSystem.Terminate().Wait();
            _logger.Log(LogLevel.Info, $"Simulation run for { sim.ActorSystem.Uptime } and ended!");
        }
    }
}
