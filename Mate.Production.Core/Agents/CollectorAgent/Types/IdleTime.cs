using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;

namespace Mate.Production.Core.Agents.CollectorAgent.Types
{
    public class OperationInformationManager
    {
        private readonly Dictionary<string, List<OperationInfo>> _operationsInfos;
        private readonly Dictionary<string, List<OperationInfo>> _operationsInfosTotal;

        public OperationInformationManager()
        {
            _operationsInfos = new Dictionary<string, List<OperationInfo>>();
            _operationsInfosTotal = new Dictionary<string, List<OperationInfo>>();
        }

        public void WriteCSV()
        {
            foreach(var cap in _operationsInfos)
            {
                LinqExtensions.WriteCSV((IEnumerable<List<OperationInfo>>)cap.Value, " " + cap);
            }
        }

        public void Add(OperationInfo operationInfo)
        {
            if(_operationsInfos.TryGetValue(operationInfo.CapabilityName, out List<OperationInfo> operationInfos))
            {
                operationInfos.Add(operationInfo);
                return;
            }
            _operationsInfos.Add(operationInfo.CapabilityName,new List<OperationInfo>() { operationInfo });
        }

        private void AddToTotals(string key, List<OperationInfo> operationInfo)
        {
            if(_operationsInfosTotal.TryGetValue(key, out List<OperationInfo> operationInfos))
            {
                operationInfos.AddRange(operationInfo);
                return;
            }
            _operationsInfosTotal.Add(key, operationInfo);
        }

        public List<Kpi> GetKpis(Collector collector, bool finalCall)
        {
            List<Kpi> kpis = new();
            // change source on final call
            var sourceDict = _operationsInfos;
            if (finalCall)
                sourceDict = _operationsInfosTotal;
            
            kpis.AddRange(
                sourceDict.Select(x => new Kpi
                {
                    Name = "Idle:" + x.Key,
                    Value = (double) Math.Round(x.Value.Average(a => a.GetIdleTime()), 2),
                    Time = (int) collector.Time,
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
                _operationsInfos.ForEach(x => AddToTotals(x.Key, x.Value));
                _operationsInfosTotal.Clear();
            }
            return kpis;
        }
    }
}
