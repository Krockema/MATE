using System;
using System.Collections.Generic;
using System.Linq;
using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.SupervisorAegnt;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
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

        private List<SimulationOrder> simulationOrders = new List<SimulationOrder>();

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
                case Supervisor.Instruction.OrderProvided m: ProvideOrder((Collector)simulationMonitor, m.GetObjectFromMessage); break;
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed((Collector)simulationMonitor, m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ProvideOrder(Collector agent, FRequestItem requestItem)
        {
            if (requestItem.DueTime >= requestItem.FinishedAt)
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

            UpdateOrder(requestItem);

            openOrderParts--;
            finishedOrderParts++;
            totalOrders++;
            
        }

        private void UpdateFeed(Collector agent, bool writeResultsToDB)
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


            WriteToDB(agent);
            
            agent.Context.Sender.Tell(true, agent.Context.Self);
        }

        private void AddOrder(Collector agent, Contract.Instruction.StartOrder m)
        {
            var op = m.GetObjectFromMessage;
            var order = new SimulationOrder() { CreationTime = op.CustomerOrder.CreationTime
                                                ,DueTime = op.CustomerOrder.DueTime
                                                , State = DB.Enums.State.Created
                                                , BusinessPartnerId = op.CustomerOrder.BusinessPartnerId
                                                , Name = op.CustomerOrder.Name
                                                , OriginId  = op.CustomerOrder.Id
                                                , SimulationNumber = agent.simulationNumber.Value
                                                , SimulationType = agent.simulationKind.Value
                                                , SimulationConfigurationId = agent.simulationId.Value };  // TODO

            simulationOrders.Add(order);

            openOrderParts++;
            newOrderParts++;
        }


        private void UpdateOrder(FRequestItem item)
        {
            var toUpdate = simulationOrders.Where(x => x.OriginId == item.CustomerOrderId).SingleOrDefault();
            if (toUpdate == null)
            {
                throw new Exception("OrderNotFound during Orderupdate from Contract Collector Agent");
            }

            toUpdate.State = DB.Enums.State.Finished;
            toUpdate.FinishingTime = (int)item.FinishedAt;
        }

        private void WriteToDB(Collector agent)
        {
            if (agent.saveToDB.Value)
            {
                using (var ctx = ResultContext.GetContext(agent.Config.GetOption<DBConnectionString>().Value))
                {
                    ctx.SimulationOrders.AddRange(simulationOrders);
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }
        }
    }
}
