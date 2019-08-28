using System;
using AkkaSim;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.SupervisorAegnt;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using Newtonsoft.Json;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public class CollectorAnalyticsContracts : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsContracts() : base() { }

        private int newOrderParts = 0;
        private int finishedOrderParts = 0;
        private int openOrderParts = 0;
        private int totalOrders = 0;

        private double inTime = 0;
        private double toLate = 0;
        private long lastIntervalStart = 0;

        public static CollectorAnalyticsContracts Get()
        {
            return new CollectorAnalyticsContracts();
        }

        public override bool Action(Agent agent, object message) => throw new Exception("Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case Contract.Instruction.StartOrder m: AddOrder((Collector)simulationMonitor, m); break;
                case Supervisor.Instruction.OrderProvided m: ProvideOrder((Collector)simulationMonitor, m); break;
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed((Collector)simulationMonitor); break;
                default: return false;
            }
            return true;
        }

        private void ProvideOrder(Collector agent, Supervisor.Instruction.OrderProvided m)
        {
            var message = m.Message as FRequestItem;
            if (message.DueTime >= message.FinishedAt)
            {
                // agent.messageHub.SendToAllClients("Oder No:" + message.OrderId  + " finished in time at " + agent.Time
                //                                 , MessageSystem.Messages.MessageType.success);

                agent.messageHub.SendToClient("orderListener", totalOrders.ToString());
                inTime++;
            }else
            {
                // agent.messageHub.SendToAllClients("Oder No:" + message.OrderId + " finished to late at " + agent.Time
                //                                 , MessageSystem.Messages.MessageType.warning);
                toLate++;
            }
            openOrderParts--;
            finishedOrderParts++;
            totalOrders++;
        }

        private void Collect(Collector agent, object message)
        {
            //agent.ActorPaths.
        }

        private void UpdateFeed(Collector agent)
        {
            //var open = openOrderParts.GroupBy(x => x.Article).Select(y => new { Article =  y.Key, Count = y.Sum(z => z.Quantity)} );
           
            agent.messageHub
                 .SendToClient("Contracts", 
                                JsonConvert.SerializeObject(new { Input = newOrderParts,
                                                                  Processing = openOrderParts,
                                                                  Output = finishedOrderParts,
                                                                  Time = agent.Time }));

            if (inTime != 0)
            {
                var timeliness = (inTime / (inTime + toLate) * 100).ToString().Replace(",", ".");
                agent.messageHub
                    .SendToClient("Timeliness", timeliness);
            }
            
            newOrderParts = 0;
            finishedOrderParts = 0;
            agent.Context.Sender.Tell(true, agent.Context.Self);
        }

        private void AddOrder(Collector agent, Contract.Instruction.StartOrder m)
        {
            var op = m.GetObjectFromMessage;
            openOrderParts++;
            newOrderParts++;
        }
    }
}
