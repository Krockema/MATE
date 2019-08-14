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

        public static CollectorAnalyticsStorage Get()
        {
            return new CollectorAnalyticsStorage();
        }

        public override bool Action(Agent agent, object message) => throw new Exception("Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case FUpdateStockValue m: UpdateStock((Collector)simulationMonitor, m); break;
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed((Collector)simulationMonitor, m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void UpdateFeed(Collector agent, bool writeToDatabase)
        {
            
            agent.messageHub.SendToAllClients("(" + agent.Time + ") Update Feed from Storage");
            var groupedByType = from sw in CurrentStockValues
                                group sw by sw.Value.ArticleType into grouped
                                select new
                                {
                                    GroupName = grouped.Key,
                                    Value = grouped.Sum(x => x.Value.NewValue),
                                    Time = agent.Time
                                };

            foreach (var item in groupedByType)
            {
                agent.messageHub.SendToClient("Storage", Newtonsoft.Json.JsonConvert.SerializeObject(item));
            }

            LogToDB(agent, writeToDatabase);
            agent.Context.Sender.Tell(true, agent.Context.Self);
        }


        private void UpdateStock(Collector agent, FUpdateStockValue values)
        {
            if (CurrentStockValues.ContainsKey(values.StockName))
                CurrentStockValues.Remove(values.StockName);

            CurrentStockValues.Add(values.StockName, values);
            UpdateKPI(agent, values);
        }

        private void UpdateKPI(Collector agent, FUpdateStockValue values)
        {
            var k = new Kpi { Name = values.StockName
                            , Value = values.NewValue
                            , Time = (int)agent.Time
                            , KpiType = DB.Enums.KpiType.StockEvolution
                            , SimulationConfigurationId = agent.simulationId.Value
                            , SimulationNumber = agent.simulationNumber.Value
                            , IsFinal = false
                            , IsKpi = true
                            , SimulationType = agent.simulationKind.Value
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
