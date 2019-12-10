﻿using AkkaSim;
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
using Master40.DB.Enums;
using Master40.DB.Nominal;
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
        private List<Kpi> Kpis = new List<Kpi>();
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

        public override bool Action(object message) => throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case Contract.Instruction.StartOrder m: AddOrder(m: m); break;
                case Supervisor.Instruction.OrderProvided m: ProvideOrder(finishedArticle: m.GetObjectFromMessage); break;
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed(writeResultsToDB: m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ProvideOrder(FArticle finishedArticle)
        {
            if (finishedArticle.DueTime >= finishedArticle.FinishedAt)
            {
                Collector.messageHub.SendToAllClients(msg: $"Order No: {finishedArticle.OriginRequester.Path.Name} finished {finishedArticle.Article.Name} in time at {Collector.Time}");
                inTime++;
            }
            else
            {
                Collector.messageHub.SendToAllClients(msg: $"Order No: {finishedArticle.OriginRequester.Path.Name} finished {finishedArticle.Article.Name} too late at {Collector.Time}");
                toLate++;
            }

            UpdateOrder(item: finishedArticle);

            openOrderParts--;
            finishedOrderParts++;
            totalOrders++;

            Collector.messageHub.SendToClient(listener: "orderListener", msg: totalOrders.ToString());

        }

        private void UpdateFeed(bool writeResultsToDB)
        {
            //var open = openOrderParts.GroupBy(x => x.Article).Select(y => new { Article =  y.Key, Count = y.Sum(z => z.Quantity)} );
            //Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Update Feed from Contracts");

            Collector.messageHub
                 .SendToClient(listener: "Contracts", 
                                msg: JsonConvert.SerializeObject(value: new { Input = newOrderParts,
                                                                  Processing = openOrderParts,
                                                                  Output = finishedOrderParts,
                                                                  Time = Collector.Time }));


            var timelines = 1.0;
            if (inTime != 0)
            {
                timelines = (inTime / (inTime + toLate) * 100);
                Collector.messageHub.SendToClient(listener: "Timeliness", msg: timelines.ToString().Replace(oldValue: ",", newValue: "."));
            }

            newOrderParts = 0;
            finishedOrderParts = 0;

            if (writeResultsToDB)
            {
                Kpis.Add(new Kpi
                {
                    Name = "timeliness",
                    Value = Math.Round(timelines, 2),
                    Time = (int) Collector.Time,
                    KpiType = KpiType.Timeliness,
                    SimulationConfigurationId = Collector.simulationId.Value,
                    SimulationNumber = Collector.simulationNumber.Value,
                    IsFinal = true,
                    IsKpi = true,
                    SimulationType = Collector.simulationKind.Value
                });

                Kpis.Add(new Kpi
                {
                    Name = "OrderProcessed",
                    Value = totalOrders,
                    ValueMax = openOrderParts,
                    Time = (int) Collector.Time,
                    KpiType = KpiType.Timeliness,
                    SimulationConfigurationId = Collector.simulationId.Value,
                    SimulationNumber = Collector.simulationNumber.Value,
                    IsFinal = true,
                    IsKpi = true,
                    SimulationType = Collector.simulationKind.Value
                });

            }
            WriteToDB(agent: Collector, writeResultsToDB: writeResultsToDB);

            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finished Update Feed from Contracts");
        }

        private void AddOrder(Contract.Instruction.StartOrder m)
        {
            var op = m.GetObjectFromMessage;
            var order = new SimulationOrder() { CreationTime = op.CustomerOrder.CreationTime
                                                ,DueTime = op.CustomerOrder.DueTime
                                                , State = State.Created
                                                , BusinessPartnerId = op.CustomerOrder.BusinessPartnerId
                                                , Name = op.CustomerOrder.Name
                                                , OriginId  = op.CustomerOrder.Id
                                                , SimulationNumber = Collector.simulationNumber.Value
                                                , SimulationType = Collector.simulationKind.Value
                                                , SimulationConfigurationId = Collector.simulationId.Value };  // TODO

            simulationOrders.Add(item: order);

            openOrderParts++;
            newOrderParts++;
        }


        private void UpdateOrder(FArticle item)
        {
            var toUpdate = simulationOrders.SingleOrDefault(predicate: x => x.OriginId == item.CustomerOrderId);
            if (toUpdate == null)
            {
                throw new Exception(message: "OrderNotFound during Order update from Contract Collector Agent");
            }

            toUpdate.State = State.Finished;
            toUpdate.ProductionFinishedTime = (int) item.ProvidedAt;
            toUpdate.StockExchangeGuid = item.StockExchangeId;
            toUpdate.FinishingTime = (int) item.FinishedAt;
        }

        private void WriteToDB(Collector agent,bool writeResultsToDB)
        {
            if (agent.saveToDB.Value && writeResultsToDB)
            {
                using (var ctx = ResultContext.GetContext(resultCon: agent.Config.GetOption<DBConnectionString>().Value))
                {
                    ctx.SimulationOrders.AddRange(entities: simulationOrders);
                    ctx.SaveChanges();
                    ctx.Kpis.AddRange(Kpis);
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }
        }
    }
}
