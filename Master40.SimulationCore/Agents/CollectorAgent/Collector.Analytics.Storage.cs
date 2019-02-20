using AkkaSim;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Master40.SimulationCore.Agents.Collector.Instruction;

namespace Master40.SimulationCore.Agents
{
    public class CollectorAnalyticsStorage : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsStorage() : base() {
            CurrentStockValues = new Dictionary<string, double>();
        }
        private Dictionary<string, double> CurrentStockValues;
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
                case UpdateLiveFeed m: UpdateFeed((Collector)simulationMonitor); break;
                default: return false;
            }
            return true;
        }

        private void UpdateFeed(Collector agent)
        {
            Debug.WriteLine("(" + agent.Time + ") Update Feed from Storage", "Collector.Storage");
            foreach (var item in CurrentStockValues)
            {
                Debug.WriteLine(item.Key + ": " + item.Value, "Collector.Storage");
            }


            lastIntervalStart = agent.Time;
            agent.Context.Sender.Tell(true, agent.Context.Self);
        }


        private void UpdateStock(Collector agent, UpdateStockValues values)
        {
            if (CurrentStockValues.ContainsKey(values.StockName))
                CurrentStockValues.Remove(values.StockName);

            CurrentStockValues.Add(values.StockName, values.NewValue);
        }
    }
}
