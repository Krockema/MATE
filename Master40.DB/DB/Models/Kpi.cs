using Master40.DB.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Models
{
    public class Kpi : BaseEntity
    {
        public double Value { get; set; }
        public string Name { get; set; }
        public bool IsKpi { get; set; }
        public KpyType KpiType {get ; set; }
        public int SimulationConfigurationId { get; set; }
        public string SimulationType { get; set; }
        public int SimulationNumber { get; set; }

    }
}
