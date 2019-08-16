using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.SupervisorAgent;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static FArticles;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

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

        public Collector Collector { get; set; }

        public static CollectorAnalyticsContracts Get()
        {
            return new CollectorAnalyticsContracts();
        }

        internal static List<Type> GetStreamTypes()
        {
            return new List<Type> { typeof(Contract.Instruction.StartOrder),
                                    typeof(Supervisor.Instruction.OrderProvided),
                                    typeof(UpdateLiveFeed)};
        }

        public override bool Action(object message) => throw new Exception("Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case Contract.Instruction.StartOrder m: AddOrder(m); break;
                case Supervisor.Instruction.OrderProvided m: ProvideOrder(m.GetObjectFromMessage); break;
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed(m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ProvideOrder(FArticle requestItem)
        {
            if (requestItem.DueTime >= requestItem.FinishedAt)
            {
                Collector.messageHub.SendToAllClients("Oder No:" + requestItem.OriginRequester  + " finished in time at " + Collector.Time);

                Collector.messageHub.SendToClient("orderListener", totalOrders.ToString());
                inTime++;
            }else
            {
                Collector.messageHub.SendToAllClients("Oder No:" + requestItem.OriginRequester + " finished to late at " + Collector.Time);
                toLate++;
            }

            UpdateOrder(requestItem);

            openOrderParts--;
            finishedOrderParts++;
            totalOrders++;
            
        }

        private void UpdateFeed(bool writeResultsToDB)
        {
            //var open = openOrderParts.GroupBy(x => x.Article).Select(y => new { Article =  y.Key, Count = y.Sum(z => z.Quantity)} );

            Collector.messageHub
                 .SendToClient("Contracts", 
                                JsonConvert.SerializeObject(new { Input = newOrderParts,
                                                                  Processing = openOrderParts,
                                                                  Output = finishedOrderParts,
                                                                  Time = Collector.Time }));

            if (inTime != 0)
            {
                var timeliness = (inTime / (inTime + toLate) * 100).ToString().Replace(",", ".");
                Collector.messageHub
                    .SendToClient("Timeliness", timeliness);
            }

            newOrderParts = 0;
            finishedOrderParts = 0;


            WriteToDB(Collector, writeResultsToDB);

            Collector.Context.Sender.Tell(true, Collector.Context.Self);
        }

        private void AddOrder(Contract.Instruction.StartOrder m)
        {
            var op = m.GetObjectFromMessage;
            var order = new SimulationOrder() { CreationTime = op.CustomerOrder.CreationTime
                                                ,DueTime = op.CustomerOrder.DueTime
                                                , State = DB.Enums.State.Created
                                                , BusinessPartnerId = op.CustomerOrder.BusinessPartnerId
                                                , Name = op.CustomerOrder.Name
                                                , OriginId  = op.CustomerOrder.Id
                                                , SimulationNumber = Collector.simulationNumber.Value
                                                , SimulationType = Collector.simulationKind.Value
                                                , SimulationConfigurationId = Collector.simulationId.Value };  // TODO

            simulationOrders.Add(order);

            openOrderParts++;
            newOrderParts++;
        }


        private void UpdateOrder(FArticle item)
        {
            var toUpdate = simulationOrders.SingleOrDefault(x => x.OriginId == item.CustomerOrderId);
            if (toUpdate == null)
            {
                throw new Exception("OrderNotFound during Order update from Contract Collector Agent");
            }

            toUpdate.State = DB.Enums.State.Finished;
            toUpdate.FinishingTime = (int)item.FinishedAt;
        }

        private void WriteToDB(Collector agent,bool writeResultsToDB)
        {
            if (agent.saveToDB.Value && writeResultsToDB)
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
