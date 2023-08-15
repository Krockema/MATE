using System;
using Mate.DataCore.Nominal;

namespace Mate.DataCore.ReportingModel
{
    public class Kpi : ResultBaseEntity
    {
        public double Value { get; set; }
        public double ValueMin { get; set; }
        public double ValueMax { get; set; }
        public double Count { get; set; }
        public string Name { get; set; }
        public bool IsKpi { get; set; }
        public bool IsFinal { get; set; }
        public KpiType KpiType {get ; set; }
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public DateTime Time { get; set; }
    }
}
