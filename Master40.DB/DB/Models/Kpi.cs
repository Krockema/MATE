using Master40.DB.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Models
{
    public class Kpi : BaseEntity
    {
        public double Value { get; set; }
        public double ValueMin { get; set; }
        public double ValueMax { get; set; }
        public double Count { get; set; }
        public string Name { get; set; }
        public bool IsKpi { get; set; }
        public KpiType KpiType {get ; set; }
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public int Time { get; set; }
    }
}
