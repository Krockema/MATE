using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Hive.Actors;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;
using Mate.Production.Core.Agents.ContractAgent;
using Mate.Production.Core.Agents.SupervisorAgent;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Types;
using Newtonsoft.Json;
using static FArticles;
using static Mate.Production.Core.Agents.CollectorAgent.Collector.Instruction;

namespace Mate.Production.Core.Agents.CollectorAgent
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

        public bool EventHandle(MessageMonitor simulationMonitor, object message)
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
                orderDictionary[OrderKpi.OrderState.InDue].Increment();
                orderDictionary[OrderKpi.OrderState.InDueTotal].Increment();
            }
            else
            {
                message = $"Order No: {finishedArticle.OriginRequester.Path.Name} finished {finishedArticle.Article.Name} too late at {Collector.Time}";
                orderDictionary[OrderKpi.OrderState.OverDue].Increment();
                orderDictionary[OrderKpi.OrderState.OverDueTotal].Increment();
            }
            

            Collector.messageHub.SendToAllClients(msg: message);
            UpdateOrder(item: finishedArticle);

            orderDictionary[OrderKpi.OrderState.Open].Decrement(); 
            orderDictionary[OrderKpi.OrderState.Finished].Increment();
            orderDictionary[OrderKpi.OrderState.Total].Increment(); 

            var percent = Math.Round((decimal) Collector.Time.ToSimulationTime() / Collector.maxTime * 100, 0);
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
            
            CreateKpis(finalCall);
            WriteToDb(agent: Collector, finalCall: finalCall);

            orderDictionary[OrderKpi.OrderState.New].ToZero();
            orderDictionary[OrderKpi.OrderState.Finished].ToZero();
            orderDictionary[OrderKpi.OrderState.InDue].ToZero();
            orderDictionary[OrderKpi.OrderState.OverDue].ToZero();

            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finished Update Feed from Contracts");
        }

        private void CreateKpis(bool finalCall)
        {   
            
            var from = Collector.Config.GetOption<KpiTimeSpan>().Value;
            if (finalCall)
                from = Collector.Config.GetOption<SettlingStart>().Value;

            var orders = _simulationOrders.Where(x => x.FinishingTime != 0 &&
                                                      x.FinishingTime > from);

            if (orders.Any())
            {
                CalculateAdherenceToDue(finalCall);
                CalculateTardiness(orders);
                CalculateCycleTime(orders);    
            }
            
            foreach (var value in orderDictionary)
            {
                this.Collector.Kpis.Add(new Kpi
                {
                    Name = value.Key.ToString(),
                    Value = (double)value.Value.GetValue(),
                    Time = (int) Collector.Time.ToSimulationTime(),
                    KpiType = KpiType.AdherenceToDue,
                    SimulationConfigurationId = Collector.simulationId.Value,
                    SimulationNumber = Collector.simulationNumber.Value,
                    IsFinal = finalCall,
                    IsKpi = true,
                    SimulationType = Collector.simulationKind.Value
                });
            }
        }

        private void CalculateCycleTime(IEnumerable<SimulationOrder> orders)
        {
            decimal cycleTime = (decimal)orders.Average(x => x.ProductionFinishedTime - x.CreationTime);
            orderDictionary[OrderKpi.OrderState.CycleTime].SetValue(new Quantity(cycleTime));

        }

        private void CalculateAdherenceToDue(bool finalCall)
        {
            var inDue = orderDictionary[OrderKpi.OrderState.InDue].GetValue();
            var overDue = orderDictionary[OrderKpi.OrderState.OverDue].GetValue();
            decimal adherenceToDue = 0;
            if (finalCall) 
            {
                inDue = orderDictionary[OrderKpi.OrderState.InDueTotal].GetValue();
                overDue = orderDictionary[OrderKpi.OrderState.OverDueTotal].GetValue();
            }
           
            if (inDue != 0)
            {
                adherenceToDue = inDue / (overDue + inDue) * 100;
            }

            this.Collector.Kpis.Add(new Kpi
            {
                Name = KpiType.AdherenceToDue.ToString(),
                Value = (double)Math.Round(adherenceToDue, 2),
                Time = (int) Collector.Time.ToSimulationTime(),
                KpiType = KpiType.AdherenceToDue,
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                IsFinal = finalCall,
                IsKpi = true,
                SimulationType = Collector.simulationKind.Value
            });
        }

        private void CalculateTardiness(IEnumerable<SimulationOrder> orders)
        {
            decimal lateness = (decimal)orders.Average(x => x.ProductionFinishedTime - x.DueTime);
            decimal tardinessOrders = orders.Sum(x => new[] {0, x.ProductionFinishedTime - x.DueTime}.Max());
            decimal tardinessCount = orders.Count(x => 0 > x.ProductionFinishedTime - x.DueTime);

            decimal tardiness = 0;
            if (tardinessCount > 0)
                tardiness = tardinessOrders / tardinessCount;
            
            orderDictionary[OrderKpi.OrderState.Lateness].SetValue(new Quantity(lateness));
            orderDictionary[OrderKpi.OrderState.Tardiness].SetValue(new Quantity(tardiness));            
        }


        private void AddOrder(Contract.Instruction.StartOrder m)
        {
            var op = m.GetObjectFromMessage;
            var order = new SimulationOrder() { CreationTime = op.CustomerOrder.CreationTime
                                                , DueTime = op.CustomerOrder.DueTime
                                                , State = State.Created
                                                , Quantity = op.CustomerOrder.CustomerOrderParts.ToArray()[0].Quantity
                                                , BusinessPartnerId = op.CustomerOrder.BusinessPartnerId
                                                , Name = op.CustomerOrder.Name
                                                , OriginId  = op.CustomerOrder.Id
                                                , SimulationNumber = Collector.simulationNumber.Value
                                                , SimulationType = Collector.simulationKind.Value
                                                , SimulationConfigurationId = Collector.simulationId.Value };

            _simulationOrders.Add(item: order);

            
            orderDictionary[OrderKpi.OrderState.Open].Increment();
            orderDictionary[OrderKpi.OrderState.New].Increment();
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
                using var ctx = MateResultDb.GetContext(resultCon: agent.Config.GetOption<ResultsDbConnectionString>().Value);
                ctx.SimulationOrders.AddRange(entities: _simulationOrders);
                ctx.SaveChanges();
                ctx.Kpis.AddRange(this.Collector.Kpis);
                ctx.SaveChanges();
                ctx.Dispose();
            }
        }
    }
}
