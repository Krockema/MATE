using System.Collections.Generic;
using System.Linq;
using Akka.Hive.Definitions;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;

namespace Mate.Production.Core.Helper
{
    public class ResultAggregator 
    {
        private readonly MateResultDb _resultCtx;

        /*
            -- Timeliness
            select * into #Timeliness from (select SimulationNumber, Value as 'Timeliness' from Kpis where KpiType = 3 and Name = 'timeliness') as x
            -- Bound Capital
            select * into #StockValue from (select Sum(Value) as 'StockValue', SimulationNumber from Kpis where KpiType = 10 and Name in ('Assembly', 'Product') group by SimulationNumber) as x
            --ThroughputTime
             select * into #ThroughputTime from (select SimulationNumber, Value as 'ThroughPutTime' from Kpis where KpiType = 0) as x 
            -- Utilization
            select * into #Utilization from (select  SimulationNumber,AVG(Value) as 'UtilizationAverage' from kpis
	  	              where (KpiType = 8)
		              group by SimulationNumber) as x1
            -- Setup
            select * into #Setup from (select  SimulationNumber,AVG(Value) as 'SetupAverage' from kpis
	  	              where (KpiType = 9)
		              group by SimulationNumber) as x
            -- Bound Capital
            select * into #Orders from (select Value as 'OrdersProcessed', ValueMax as 'OrdersOpen', SimulationNumber from Kpis where KpiType = 3 and Name = 'OrderProcessed') as x
         */


        public ResultAggregator(MateResultDb resultCtx)
        {
            _resultCtx = resultCtx;
        }

        public void BuildResults(int forId)
        {
            // return new Task(() =>
            // {
                var toRemove = _resultCtx.Kpis.Where(x =>
                    x.KpiType == KpiType.AverageResult && x.SimulationConfigurationId == forId);
                _resultCtx.Kpis.RemoveRange(toRemove);
                _resultCtx.SaveChanges();

                var toAdd = new List<Kpi>();
                toAdd.Add(Timeliness(forId));
                toAdd.Add(StockTotals(forId));
                toAdd.Add(ThroughPutTimes(forId));
                toAdd.Add(Utilization(forId));
                toAdd.Add(Setup(forId));
                toAdd.Add(CompletedOrders(forId));

                _resultCtx.Kpis.AddRange(toAdd);
                _resultCtx.SaveChanges();
            // });
        }

        private Kpi Timeliness(int forId)
        {
            var values = _resultCtx.Kpis
                .Where(x => x.KpiType == KpiType.AdherenceToDue
                         && x.SimulationConfigurationId == forId
                         && x.Name == "timeliness")
                .Select(x => x.Value);

            return KpiCreatorAverage(forId, values.ToList(), "Timeliness");
        }

        private Kpi StockTotals(int forId)
        {
            // select Sum(Value) as 'StockValue', SimulationNumber from Kpis where KpiType = 10 and Name in ('Assembly', 'Product') group by SimulationNumber
            
            var preSelection = _resultCtx.Kpis
                .Where(x => x.KpiType == KpiType.StockTotals
                            && x.SimulationConfigurationId == forId
                            && (x.Name == "Product" || x.Name == "Assembly"))
                .Select(x =>  new { x.SimulationNumber, x.Value })
                .ToList();


            var values = from k in preSelection
                group k by k.SimulationNumber
                into g
                orderby g.Key
                select (double) g.Sum(s => s.Value);

            return KpiCreatorAverage(forId, values.ToList(), "StockTotals");
        }

        private Kpi ThroughPutTimes(int forId)
        {
            // select * into #ThroughputTime from (select SimulationNumber, Value as 'ThroughPutTime' from Kpis where KpiType = 0) as x 
            var values = _resultCtx.Kpis
                .Where(x => x.KpiType == KpiType.LeadTime
                            && x.SimulationConfigurationId == forId)
                .Select(x => x.Value);

            return KpiCreatorAverage(forId, values.ToList(), "ThroughPutTime");
        }

        private Kpi Utilization(int forId)
        {
            // select * into #Utilization from (select  SimulationNumber,AVG(Value) as 'UtilizationAverage' from kpis
            // where (KpiType = 8)
            // group by SimulationNumber) as x

            var preSelection = _resultCtx.Kpis
                .Where(x => x.KpiType == KpiType.ResourceUtilizationTotal
                            && x.SimulationConfigurationId == forId)
                .Select(x =>  new { x.SimulationNumber, x.Value })
                .ToList();


            var values = from k in preSelection
                group k by k.SimulationNumber
                into g
                orderby g.Key
                select (double) g.Average(s => s.Value);

            return KpiCreatorAverage(forId, values.ToList(), "Utilization");
        }

        private Kpi Setup(int forId)
        {
            // select * into #Utilization from (select  SimulationNumber,AVG(Value) as 'UtilizationAverage' from kpis
            // where (KpiType = 8)
            // group by SimulationNumber) as x

            var preSelection = _resultCtx.Kpis
                .Where(x => x.KpiType == KpiType.ResourceSetupTotal
                            && x.SimulationConfigurationId == forId)
                .Select(x =>  new { x.SimulationNumber, x.Value })
                .ToList();


            var values = from k in preSelection
                group k by k.SimulationNumber
                into g
                orderby g.Key
                select (double) g.Average(s => s.Value);

            return KpiCreatorAverage(forId, values.ToList(), "Setup");
        }

        private Kpi CompletedOrders(int forId)
        {
            var values = _resultCtx.Kpis
                .Where(x => x.KpiType == KpiType.AdherenceToDue
                            && x.SimulationConfigurationId == forId
                            && x.Name == "OrderProcessed")
                .Select(x => x.Value);

            return KpiCreatorAverage(forId, values.ToList(), "CompletedOrders");
        }
        
        private Kpi KpiCreatorAverage(int forId, List<double> values , string name)
        {
            return new Kpi()
            {
                Name = name,
                KpiType = KpiType.AverageResult,
                Count = values.Count(), // may count simulation runs ? 
                IsFinal = true,
                IsKpi = true,
                SimulationConfigurationId = forId,
                SimulationNumber = 0,
                SimulationType = 0,
                Time = Time.ZERO.Value,
                ValueMax = values.Max(),
                ValueMin = values.Min(),
                Value = values.Average(),
            };
        }
    }
}