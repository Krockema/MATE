using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.CollectorAgent.Types
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
                quantities.Add(new Quantity((decimal)(job.Start - job.ReadyAt).TotalMinutes));
                return;
            }
            _idleDictionary.Add(job.CapabilityName,new List<Quantity>() { new Quantity((decimal)(job.Start - job.ReadyAt).TotalMinutes) });
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

        public List<Kpi> GetKpis(Collector collector, bool finalCall)
        {
            List<Kpi> kpis = new();
            // change source on final call
            var sourceDict = _idleDictionary;
            if (finalCall)
                sourceDict = _idleDictionaryTotals;
            
            kpis.AddRange(
                sourceDict.Select(x => new Kpi
                {
                    Name = "Idle:" + x.Key,
                    Value = (double) Math.Round(x.Value.Average(a => a.GetValue()), 2),
                    Time = collector.Time.Value,
                    KpiType = KpiType.CapabilityIdle,
                    SimulationConfigurationId = collector.simulationId.Value,
                    SimulationNumber = collector.simulationNumber.Value,
                    IsFinal = finalCall,
                    IsKpi = true,
                    SimulationType = collector.simulationKind.Value
                }));
            
            if (!finalCall)
            { 
                // clear if more is coming.
                _idleDictionary.ForEach(x => AddToTotals(x.Key, x.Value));
                _idleDictionary.Clear();
            }
            return kpis;
        }
    }
}
