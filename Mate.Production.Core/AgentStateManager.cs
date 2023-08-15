using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hive;
using Akka.Hive.Actors;
using Akka.Hive.Logging;
using Mate.Production.Core.Interfaces;
using NLog;
using static Mate.Production.Core.Agents.CollectorAgent.Collector.Instruction;

namespace Mate.Production.Core
{
    public class AgentStateManager : StateManager, IStateManager
    {
        private readonly List<IActorRef> _collectorRefs;
        private readonly Logger _logger;


        public static Props Props(List<IActorRef> collectorRefs, Hive hive)
        {
            return Akka.Actor.Props.Create(() => new AgentStateManager(collectorRefs, hive));
        }

        public AgentStateManager(List<IActorRef> collectorRefs, Hive hive) : base(hive)
        {
            _collectorRefs = collectorRefs;
            _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
        }

        public override void AfterSimulationStarted()
        {
            //base.AfterSimulationStarted(sim);
        }

        public override void AfterSimulationStopped()
        {
            var tasks = new List<Task>();
            foreach (var item in _collectorRefs)
            {
                var msg = UpdateLiveFeed.Create(setup: false, target: Self);
                _logger.Log(LogLevel.Info, $"Ask for Update Feed {item.Path.Name}");
                tasks.Add(item.Ask(message: msg, timeout: TimeSpan.FromSeconds(value: 60 * 60)));
                        
            }
            Task.WaitAll(tasks.ToArray(), timeout: TimeSpan.FromSeconds(value: 60 * 10));
            
        }

        public override void SimulationIsTerminating()
        {

            _collectorRefs.ForEach(item =>
            {
                item.Ask(message: UpdateLiveFeed.Create(setup: true, target: Self)
                       , timeout: TimeSpan.FromMinutes(value: 15)).Wait();

            });
        }
    }
}
