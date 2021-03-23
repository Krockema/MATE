using System;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.ReportingModel;
using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using Master40.DB.Nominal;

namespace Master40.SimulationCore.Agents.CollectorAgent.Types
{
    public class IdleTime
    {
        private readonly Dictionary<string, List<Quantity>> _idleDictionary;
        private readonly Dictionary<string, List<Quantity>> _idleDictionaryTotals;

        public IdleTime()
        {
            _idleDictionary = new Dictionary<string, List<Quantity>>();
            _idleDictionaryTotals = new Dictionary<string, List<Quantity>>();
        }

        public void Add(Job job)
        {
            if(_idleDictionary.TryGetValue(job.CapabilityName, out List<Quantity> quantities))
            {
                quantities.Add(new Quantity(job.Start - job.ReadyAt));
                return;
            }
            _idleDictionary.Add(job.CapabilityName,new List<Quantity>() { new Quantity(job.Start - job.ReadyAt) });
        }

        private void AddToTotals(string key, List<Quantity> idleList)
        {
            if(_idleDictionaryTotals.TryGetValue(key, out List<Quantity> quantities))
            {
                quantities.AddRange(idleList);
                return;
            }
            _idleDictionaryTotals.Add(key, idleList);
        }

        public void GetKpis(Collector collector, bool finalCall)
        {
            // change source on final call
            var sourceDict = _idleDictionary;
            if (finalCall)
                sourceDict = _idleDictionaryTotals;
            
            collector.Kpis.AddRange(
                sourceDict.Select(x => new Kpi
                {
                    Name = "Idle:" + x.Key,
                    Value = (double) Math.Round(x.Value.Average(a => a.GetValue()), 2),
                    Time = (int) collector.Time,
                    KpiType = KpiType.CapabilityIdle,
                    SimulationConfigurationId = collector.simulationId.Value,
                    SimulationNumber = collector.simulationNumber.Value,
                    IsFinal = finalCall,
                    IsKpi = true,
                    SimulationType = collector.simulationKind.Value
                }));
            
            if (finalCall) return;
            // clear if more is coming.
            _idleDictionary.ForEach(x => AddToTotals(x.Key, x.Value));
            _idleDictionary.Clear();
        }
    }
}
