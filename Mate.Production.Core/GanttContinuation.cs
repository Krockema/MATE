using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hive;
using Akka.Hive.Actors;
using Akka.Hive.Logging;
using Mate.Production.Core.Agents.HubAgent;
using Mate.Production.Core.Interfaces;
using Mate.Production.Core.SignalR;
using NLog;
using static Mate.Production.Core.Agents.CollectorAgent.Collector.Instruction;

namespace Mate.Production.Core
{
    public class GanttStateManager : StateManager, IStateManager
    {
        private readonly List<IActorRef> _collectorRefs;
        private readonly Logger _logger;
        private readonly IMessageHub _messageHub;

        public GanttStateManager(List<IActorRef> collectorRefs, IMessageHub messageHub, Hive hive) : base(hive)
        {
            _collectorRefs = collectorRefs;
            _messageHub = messageHub;
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

            var hubActorRef = base.GetActorFromString("/user/HubDirectory/CentralHub"); 
            var instruction = Hub.Instruction.Central.LoadProductionOrders.Create(Self, hubActorRef);
            base.ContextManagerRef.Tell(instruction);

            Task.WaitAll(tasks.ToArray());
            //TODO: might need to extend timespan
            // var results = _inbox.ReceiveWhere(x => x is FCentralGanttPlanInformation, TimeSpan.FromSeconds(60 * 60)) as FCentralGanttPlanInformation;
            //_messageHub.SendToClient("ganttListener", results.InfoJson);
        }

        public override void SimulationIsTerminating()
        {
            foreach (var item in _collectorRefs)
            {
                item.Ask(message: UpdateLiveFeed.Create(setup: true, target: Self)
                       , timeout: TimeSpan.FromHours(value: 1)).Wait();
            }
        }
    }
}
