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

        public override bool Action(object message) => throw new Exception("Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case FUpdateStockValue m: UpdateStock(m); break;
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed(m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void UpdateFeed(bool writeToDatabase)
        {

            Collector.messageHub.SendToAllClients("(" + Collector.Time + ") Update Feed from Storage");
            var groupedByType = from sw in CurrentStockValues
                                group sw by sw.Value.ArticleType into grouped
                                select new
                                {
                                    GroupName = grouped.Key,
                                    Value = grouped.Sum(x => x.Value.NewValue),
                                    Time = Collector.Time
                                };

            foreach (var item in groupedByType)
            {
                Collector.messageHub.SendToClient("Storage", Newtonsoft.Json.JsonConvert.SerializeObject(item));
            }

            LogToDB(Collector, writeToDatabase);
            Collector.Context.Sender.Tell(true, Collector.Context.Self);
        }


        private void UpdateStock(FUpdateStockValue values)
        {
            if (CurrentStockValues.ContainsKey(values.StockName))
                CurrentStockValues.Remove(values.StockName);

            CurrentStockValues.Add(values.StockName, values);
            UpdateKPI(values);
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
            StockValuesOverTime.Add(k);

        }

        private void LogToDB(Collector agent, bool writeToDatabase)
        {
            if (agent.saveToDB.Value && writeToDatabase)
            {
                using (var ctx = ResultContext.GetContext(agent.Config.GetOption<DBConnectionString>().Value))
                {
                    ctx.Kpis.AddRange(StockValuesOverTime);
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }
        }
    }
}
