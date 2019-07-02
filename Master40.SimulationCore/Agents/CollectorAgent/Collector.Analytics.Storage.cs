using System;
using System.Collections.Generic;
using System.Linq;
using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public class CollectorAnalyticsStorage : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsStorage() : base() {
            CurrentStockValues = new Dictionary<string, UpdateStockValues>();
        }
        private Dictionary<string, UpdateStockValues> CurrentStockValues;
        private List<Kpi> StockValuesOverTime = new List<Kpi>();

        private long lastIntervalStart = 0;


        public static CollectorAnalyticsStorage Get()
        {
            return new CollectorAnalyticsStorage();
        }

        public override bool Action(Agent agent, object message) => throw new Exception("Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case UpdateStockValues m: UpdateStock((Collector)simulationMonitor, m); break;
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed((Collector)simulationMonitor, m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void UpdateFeed(Collector agent, bool writeResultsToDB)
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


            LogToDB(agent, writeResultsToDB);
            

            lastIntervalStart = agent.Time;
            agent.Context.Sender.Tell(true, agent.Context.Self);
        }


        private void UpdateStock(Collector agent, UpdateStockValues values)
        {
            if (CurrentStockValues.ContainsKey(values.StockName))
                CurrentStockValues.Remove(values.StockName);

            CurrentStockValues.Add(values.StockName, values);
            UpdateKPI(agent, values);
        }

        private void UpdateKPI(Collector agent, UpdateStockValues values)
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

        private void LogToDB(Collector agent, bool writeResultsToDB)
        {
            if (agent.saveToDB.Value && writeResultsToDB)
            {
                using (var ctx = ResultContext.GetContext(agent.Config.GetOption<DBConnectionString>().Value))
                {
                    ctx.Kpis.AddRange(StockValuesOverTime.Select(x => { x.Id = 0; return x; }));
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }
        }
    }
}
