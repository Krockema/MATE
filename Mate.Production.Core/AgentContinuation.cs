using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaSim;
using AkkaSim.Logging;
using Mate.Production.Core.Interfaces;
using NLog;
using static Mate.Production.Core.Agents.CollectorAgent.Collector.Instruction;

namespace Mate.Production.Core
{
    public class AgentStateManager : StateManager, IStateManager
    {
        private readonly List<IActorRef> _collectorRefs;
        private readonly Inbox _inbox;
        private readonly Logger _logger;

        public AgentStateManager(List<IActorRef> collectorRefs, Inbox inbox)
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
            Task.WaitAll(tasks.ToArray(), timeout: TimeSpan.FromSeconds(value: 60 * 10));
            
        }

        public override void SimulationIsTerminating(Simulation sim)
        {
            foreach (var item in _collectorRefs)
            {
                item.Ask(message: UpdateLiveFeed.Create(setup: true, target: _inbox.Receiver)
                       , timeout: TimeSpan.FromMinutes(value: 15)).Wait();
            }
            sim.ActorSystem.Terminate().Wait();
            _logger.Log(LogLevel.Info, $"Simulation run for { sim.ActorSystem.Uptime } and ended!");
        }
    }
}
