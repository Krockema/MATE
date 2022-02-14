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


        public List<Kpi> GetIdleKpis(Collector collector, bool finalCall)
        {
            List<Kpi> kpis = new();
            // change source on final call
            var sourceDict = _operationsInfos;
            if (finalCall)
                sourceDict = _operationsInfosTotal;

            var avgIdleTime = sourceDict.Select(x => new Kpi
            {
                Name = x.Key,
                Value = (double)Math.Round(x.Value.Average(a => a.GetIdleTime()), 2),
                Time = (int)collector.Time,
                KpiType = KpiType.AvgIdleTimeCapability,
                SimulationConfigurationId = collector.simulationId.Value,
                SimulationNumber = collector.simulationNumber.Value,
                IsFinal = finalCall,
                IsKpi = true,
                SimulationType = collector.simulationKind.Value
            });

            kpis.AddRange(avgIdleTime);
            
            return kpis;
        }

        public List<Kpi> GetAmountIdleOperationKpis(Collector collector, bool finalCall)
        {
            List<Kpi> kpis = new();
            // change source on final call
            var sourceDict = _operationsInfos;
            if (finalCall)
                sourceDict = _operationsInfosTotal;

            var amountOperations = sourceDict.Select(x => new Kpi
            {
                Name = x.Key,
                Value = x.Value.Count,
                Time = (int)collector.Time,
                KpiType = KpiType.AmountIdleOperationsCapability,
                SimulationConfigurationId = collector.simulationId.Value,
                SimulationNumber = collector.simulationNumber.Value,
                IsFinal = finalCall,
                IsKpi = true,
                SimulationType = collector.simulationKind.Value
            });

            kpis.AddRange(amountOperations);

            return kpis;
        }


        public void Clear()
        {
            // clear if more is coming.
            _operationsInfos.ForEach(x => AddToTotals(x.Key, x.Value));
            _operationsInfos.Clear();
        }
    }
}
