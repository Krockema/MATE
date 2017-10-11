using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Models
{
    public class SimulationConfiguration : BaseEntity
    { 
        public string Name { get; set; }
        public int Time { get; set; }
        public int MaxCalculationTime { get; set; }
        public int Lotsize { get; set; }
        public int OrderQuantity { get; set; }
        public int Seed { get; set; }
        public int SettlingStart { get; set; }
        public int ConsecutiveRuns { get; set; }
        public int RecalculationTime { get; set; }
        public int SimulationEndTime { get; set; }
        public int CentralRuns { get; set; }
        public int DecentralRuns { get; set; }
        public double OrderRate { get; set; }
        public bool DecentralIsRunning { get; set; }
        public bool CentralIsRunning { get; set; }
        public int DynamicKpiTimeSpan { get; set; }
        public double WorkTimeDeviation { get; set; }
    }
}
