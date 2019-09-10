using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static FUpdateStockValues;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public class CollectorAnalyticsStorage : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsStorage() : base() {
            CurrentStockValues = new Dictionary<string, FUpdateStockValue>();
        }
        internal static List<Type> GetStreamTypes()
        {
            return new List<Type> { typeof(FUpdateStockValue),
                                    typeof(UpdateLiveFeed)};
        }

        private Dictionary<string, FUpdateStockValue> CurrentStockValues;
        private List<Kpi> StockValuesOverTime = new List<Kpi>();

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
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed(writeToDatabase: m.GetObjectFromMessage); break;
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
                Collector.messageHub.SendToClient(listener: "Storage", msg: Newtonsoft.Json.JsonConvert.SerializeObject(value: item));
            }


            if (writeToDatabase)
            {
                foreach (var item in CurrentStockValues)
                {
                    System.Diagnostics.Debug.WriteLine($"Storage: {item.Value.StockName} in stock {item.Value.NewValue} ");
                }
                
            }

            LogToDB(agent: Collector, writeToDatabase: writeToDatabase);
            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finish Update Feed from Storage");
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
            var k = new Kpi { Name = values.StockName
                            , Value = values.NewValue
                            , Time = (int)Collector.Time
                            , KpiType = DB.Enums.KpiType.StockEvolution
                            , SimulationConfigurationId = Collector.simulationId.Value
                            , SimulationNumber = Collector.simulationNumber.Value
                            , IsFinal = false
                            , IsKpi = true
                            , SimulationType = Collector.simulationKind.Value
            };
            StockValuesOverTime.Add(item: k);

        }

        private void LogToDB(Collector agent, bool writeToDatabase)
        {
            if (agent.saveToDB.Value && writeToDatabase)
            {
                using (var ctx = ResultContext.GetContext(resultCon: agent.Config.GetOption<DBConnectionString>().Value))
                {
                    ctx.Kpis.AddRange(entities: StockValuesOverTime);
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }
        }
    }
}
