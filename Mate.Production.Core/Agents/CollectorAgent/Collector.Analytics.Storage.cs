using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Util.Internal;
using IdentityModel;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Types;
using Newtonsoft.Json;
using static Mate.Production.Core.Agents.CollectorAgent.Collector.Instruction;

namespace Mate.Production.Core.Agents.CollectorAgent
{
    public class CollectorAnalyticsStorage : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsStorage() : base() {
            CurrentStockValues = new Dictionary<string, UpdateStockValueRecord>();
            TotalValues = new Dictionary<string, decimal>();
        }
        internal static List<Type> GetStreamTypes()
        {
            return new List<Type> { typeof(UpdateStockValueRecord),
                                    typeof(UpdateLiveFeed)};
        }

        private Dictionary<string, UpdateStockValueRecord> CurrentStockValues;
        private Dictionary<string, decimal> TotalValues;
        private List<Kpi> StockValuesOverTime = new List<Kpi>();
        private List<Kpi> StockTotalValues = new List<Kpi>();

        public Collector Collector { get; set; }

        public static CollectorAnalyticsStorage Get()
        {
            return new CollectorAnalyticsStorage();
        }

        public override bool Action(object message) => throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(MessageMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case UpdateStockValueRecord m: UpdateStock(values: m); break;
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
                    Value = summarized.Sum(x => (x.Time.ToEpochTime() - x.ValueMin) * x.Value / Collector.Time.Value.ToEpochTime() * 0.10)
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

            LogToDB(agent: Collector, writeToDatabase: writeToDatabase);
            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time.Value + ") Finish Update Feed from Storage");
        }


        private void UpdateStock(UpdateStockValueRecord values)
        {
            if (CurrentStockValues.ContainsKey(key: values.StockName))
                CurrentStockValues.Remove(key: values.StockName);

            CurrentStockValues.Add(key: values.StockName, value: values);
            UpdateKPI(values: values);
        }

        private void UpdateKPI(UpdateStockValueRecord values)
        {
            var lastTime = 0L; 
            var lastValue = StockValuesOverTime.FindAll(x => x.Name == values.ArticleType + " " + values.StockName);
            if(lastValue.Any()) { 
                lastTime = lastValue.Max(x => x.Time.ToEpochTime());
            }
            else
            {
                CreateKpiStartEntry(values);
            }

            var k = new Kpi { Name = values.ArticleType + " " + values.StockName
                            , Value = values.NewValue
                            , ValueMin = lastTime
                            , Time = Collector.Time.Value
                            , KpiType = KpiType.StockEvolution
                            , SimulationConfigurationId = Collector.simulationId.Value
                            , SimulationNumber = Collector.simulationNumber.Value
                            , IsFinal = false
                            , IsKpi = true
                            , SimulationType = Collector.simulationKind.Value
            };
            StockValuesOverTime.Add(item: k);
        }

        private void CreateKpiStartEntry(UpdateStockValueRecord values)
        {
            var k = new Kpi
            {
                Name = values.ArticleType + " " + values.StockName,
                Value = values.NewValue,
                Time = Time.ZERO.Value,
                KpiType = KpiType.StockEvolution,
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
                Time = Collector.Time.Value,
                KpiType = KpiType.StockTotals,
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
                using (var ctx = MateResultDb.GetContext(resultCon: agent.Config.GetOption<ResultsDbConnectionString>().Value))
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
