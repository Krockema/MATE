using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using Newtonsoft.Json;
using static FUpdateStockValues;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public class CollectorAnalyticsStorage : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsStorage() : base() {
            CurrentStockValues = new Dictionary<string, FUpdateStockValue>();
            TotalValues = new Dictionary<string, decimal>();
        }
        internal static List<Type> GetStreamTypes()
        {
            return new List<Type> { typeof(FUpdateStockValue),
                                    typeof(UpdateLiveFeed)};
        }

        private Dictionary<string, FUpdateStockValue> CurrentStockValues;
        private Dictionary<string, decimal> TotalValues;
        private List<Kpi> StockValuesOverTime = new List<Kpi>();
        private List<Kpi> StockTotalValues = new List<Kpi>();

        public Collector Collector { get; set; }

        public static CollectorAnalyticsStorage Get()
        {
            return new CollectorAnalyticsStorage();
        }

        public override bool Action(object message) => throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case FUpdateStockValue m: UpdateStock(values: m); break;
                case UpdateLiveFeed m: UpdateFeed(writeToDatabase: m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void UpdateFeed(bool writeToDatabase)
        {

            //Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Update Feed from Storage");
            var groupedByType = from sw in CurrentStockValues
                                group sw by sw.Value.ArticleType into grouped
                                select new
                                {
                                    GroupName = grouped.Key,
                                    Value = grouped.Sum(selector: x => x.Value.NewValue),
                                    Time = Collector.Time
                                };

            foreach (var item in groupedByType.OrderBy(x => x.Value))
            {
                Collector.messageHub.SendToClient(listener: "Storage", msg: JsonConvert.SerializeObject(value: item));
            }

            var stockTotals = from so in StockValuesOverTime
                group so by so.Name.Substring(0, 8)
                into summarized orderby summarized.Key
                select new
                {
                    Key = summarized.Key,
                    Value = summarized.Sum(x => ((x.Time - x.ValueMin) * x.Value / Collector.Time) * 0.10)
                };


            if (writeToDatabase)
            {
                foreach (var item in CurrentStockValues)
                {
                    System.Diagnostics.Debug.WriteLine($"Storage: {item.Value.StockName} in stock {item.Value.NewValue} ");
                    UpdateKPI(item.Value);
                }
                Collector.messageHub.SendToClient(listener: "stockTotalsListener", msg: JsonConvert.SerializeObject(stockTotals));
            }

            stockTotals.ForEach(st => CreateKpi(st.Key, st.Value, writeToDatabase));
            GatherKpiForAI();
            LogToDB(agent: Collector, writeToDatabase: writeToDatabase);
            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finish Update Feed from Storage");
        }

        private void GatherKpiForAI()
        {
            if (Collector.Time <= Collector.Config.GetOption<SettlingStart>().Value) return;
            var assembly = StockTotalValues.Find(k => k.Name == "Assembly" && k.Time == Collector.Time);
            var fAssembly = new FKpi.FKpi(assembly.Time, assembly.Name, assembly.Value);
            Collector.SendKpi(fAssembly);
            var consumable = StockTotalValues.Find(k => k.Name == "Consumab" && k.Time == Collector.Time);
            var fConsumable = new FKpi.FKpi(consumable.Time, consumable.Name, consumable.Value);
            Collector.SendKpi(fConsumable);
            var material = StockTotalValues.Find(k => k.Name == "Material" && k.Time == Collector.Time);
            var fMaterial = new FKpi.FKpi(material.Time, material.Name, material.Value);
            Collector.SendKpi(fMaterial);
        }


        private void UpdateStock(FUpdateStockValue values)
        {
            if (CurrentStockValues.ContainsKey(key: values.StockName))
                CurrentStockValues.Remove(key: values.StockName);

            CurrentStockValues.Add(key: values.StockName, value: values);
            UpdateKPI(values: values);
        }

        private void UpdateKPI(FUpdateStockValue values)
        {
            var lastTime = 0; 
            var lastValue = StockValuesOverTime.FindAll(x => x.Name == values.ArticleType + " " + values.StockName);
            if(lastValue.Any()) { 
                lastTime = lastValue.Max(x => x.Time);
            }
            else
            {
                CreateKpiStartEntry(values);
            }

            var k = new Kpi { Name = values.ArticleType + " " + values.StockName
                            , Value = values.NewValue
                            , ValueMin = lastTime
                            , Time = (int)Collector.Time
                            , KpiType = DB.Nominal.KpiType.StockEvolution
                            , SimulationConfigurationId = Collector.simulationId.Value
                            , SimulationNumber = Collector.simulationNumber.Value
                            , IsFinal = false
                            , IsKpi = true
                            , SimulationType = Collector.simulationKind.Value
            };
            StockValuesOverTime.Add(item: k);
        }

        private void CreateKpiStartEntry(FUpdateStockValue values)
        {
            var k = new Kpi
            {
                Name = values.ArticleType + " " + values.StockName,
                Value = values.NewValue,
                Time = 0,
                KpiType = DB.Nominal.KpiType.StockEvolution,
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                IsFinal = false,
                IsKpi = true,
                SimulationType = Collector.simulationKind.Value
            };
            StockValuesOverTime.Add(item: k);
        }

        private void CreateKpi(string name, double values, bool isFinal)
        {
            var k = new Kpi
            {
                Name = name,
                Value = values,
                Time = (int)Collector.Time,
                KpiType = DB.Nominal.KpiType.StockTotals,
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                IsFinal = isFinal,
                IsKpi = true,
                SimulationType = Collector.simulationKind.Value
            };
            StockTotalValues.Add(item: k);
        }

        private void LogToDB(Collector agent, bool writeToDatabase)
        {
            if (agent.saveToDB.Value && writeToDatabase)
            {
                using (var ctx = ResultContext.GetContext(resultCon: agent.Config.GetOption<DBConnectionString>().Value))
                {
                    // ctx.Kpis.AddRange(entities: StockValuesOverTime);
                    // ctx.SaveChanges();
                    ctx.Kpis.AddRange(StockTotalValues);
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }
        }
    }
}
