using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.Nominal;
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
        
        private OrderKpi orderDictionary = new OrderKpi();
        
        private readonly List<SimulationOrder> _simulationOrders = new List<SimulationOrder>();
        private CollectorAnalyticsContracts() : base()
        {

        }


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
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed(finalCall: m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ProvideOrder(FArticle finishedArticle)
        {
            string message;
            if (finishedArticle.DueTime >= finishedArticle.FinishedAt)
            {
                message = $"Order No: {finishedArticle.OriginRequester.Path.Name} finished {finishedArticle.Article.Name} in time at {Collector.Time}";
                orderDictionary[OrderKpi.OrderState.InDue].IncrementByOne();
            }
            else
            {
                message = $"Order No: {finishedArticle.OriginRequester.Path.Name} finished {finishedArticle.Article.Name} too late at {Collector.Time}";
                orderDictionary[OrderKpi.OrderState.OverDue].IncrementByOne();
            }
            

            Collector.messageHub.SendToAllClients(msg: message);
            UpdateOrder(item: finishedArticle);

            orderDictionary[OrderKpi.OrderState.Open].DecrementByOne(); 
            orderDictionary[OrderKpi.OrderState.Finished].IncrementByOne();
            orderDictionary[OrderKpi.OrderState.Total].IncrementByOne(); 

            var percent = Math.Round((decimal) Collector.Time / Collector.maxTime * 100, 0);
            Collector.messageHub.ProcessingUpdate((int)orderDictionary[OrderKpi.OrderState.Finished].GetValue(), (int)percent, 
                                                    $" Progress({percent} %) " + message
                                                    , (int)orderDictionary[OrderKpi.OrderState.Total].GetValue());
            var timeliness = new[] { orderDictionary[OrderKpi.OrderState.Total].GetValue().ToString(), (finishedArticle.ProvidedAt - finishedArticle.DueTime).ToString()};
            Collector.messageHub.SendToClient(listener: "orderListener", msg: JsonConvert.SerializeObject(timeliness));

        }

        private void UpdateFeed(bool finalCall)
        {
            //var open = openOrderParts.GroupBy(x => x.Article).Select(y => new { Article =  y.Key, Count = y.Sum(z => z.Quantity)} );
            //Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Update Feed from Contracts");

            Collector.messageHub
                 .SendToClient(listener: "Contracts", 
                                msg: JsonConvert.SerializeObject(value: new { Input = orderDictionary[OrderKpi.OrderState.New].GetValue(),
                                                                  Processing = orderDictionary[OrderKpi.OrderState.Open].GetValue(),
                                                                  Output = orderDictionary[OrderKpi.OrderState.Finished].GetValue(),
                                                                  Time = Collector.Time }));


            decimal adheranceToDue = 0;
            if (orderDictionary[OrderKpi.OrderState.InDue].GetValue() != 0)
            {
                adheranceToDue = orderDictionary[OrderKpi.OrderState.InDue].GetValue() / 
                            (orderDictionary[OrderKpi.OrderState.InDue].GetValue() + orderDictionary[OrderKpi.OrderState.OverDue].GetValue()) * 100;
                //Collector.messageHub.SendToClient(listener: "Timeliness", msg: timelines.ToString().Replace(oldValue: ",", newValue: "."));
            }

            this.Collector.Kpis.Add(new Kpi
            {
                Name = "AdheranceToDue",
                Value = (double)Math.Round(adheranceToDue, 2),
                Time = (int) Collector.Time,
                KpiType = KpiType.AdheranceToDue,
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                IsFinal = finalCall,
                IsKpi = true,
                SimulationType = Collector.simulationKind.Value
            });

            CreateKpis(finalCall);

            orderDictionary[OrderKpi.OrderState.New].ToZero();
            orderDictionary[OrderKpi.OrderState.Finished].ToZero();

            WriteToDb(agent: Collector, finalCall: finalCall);

            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finished Update Feed from Contracts");
        }

        private void CreateKpis(bool finalCall)
        {
            foreach (var value in orderDictionary)
            {
                this.Collector.Kpis.Add(new Kpi
                {
                    Name = value.Key.ToString(),
                    Value = (double)value.Value.GetValue(),
                    Time = (int) Collector.Time,
                    KpiType = KpiType.AdheranceToDue,
                    SimulationConfigurationId = Collector.simulationId.Value,
                    SimulationNumber = Collector.simulationNumber.Value,
                    IsFinal = finalCall,
                    IsKpi = true,
                    SimulationType = Collector.simulationKind.Value
                });
            }
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
                                                , SimulationConfigurationId = Collector.simulationId.Value };

            _simulationOrders.Add(item: order);

            
            orderDictionary[OrderKpi.OrderState.Open].IncrementByOne();
            orderDictionary[OrderKpi.OrderState.New].IncrementByOne();
        }


        private void UpdateOrder(FArticle item)
        {
            var toUpdate = _simulationOrders.SingleOrDefault(predicate: x => x.OriginId == item.CustomerOrderId);
            if (toUpdate == null)
            {
                throw new Exception(message: "OrderNotFound during Order update from Contract Collector Agent");
            }

            toUpdate.State = State.Finished;
            toUpdate.ProductionFinishedTime = (int) item.ProvidedAt;
            toUpdate.StockExchangeGuid = item.StockExchangeId;
            toUpdate.FinishingTime = (int) item.FinishedAt;
        }

        private void WriteToDb(Collector agent, bool finalCall)
        {
            if (finalCall)
                Collector.messageHub.ProcessingUpdate((int)orderDictionary[OrderKpi.OrderState.Finished].GetValue(), (int)orderDictionary[OrderKpi.OrderState.Finished].GetValue(), 
                    $"Progress({100} %) Finalizing Steps. Write to DB and Shutdown!" 
                    , (int)orderDictionary[OrderKpi.OrderState.Total].GetValue());
            
            if (agent.saveToDB.Value && finalCall)
            {
                using var ctx = ResultContext.GetContext(resultCon: agent.Config.GetOption<DBConnectionString>().Value);
                ctx.SimulationOrders.AddRange(entities: _simulationOrders);
                ctx.SaveChanges();
                ctx.Kpis.AddRange(this.Collector.Kpis);
                ctx.SaveChanges();
                ctx.Dispose();
            }
        }
    }
}
