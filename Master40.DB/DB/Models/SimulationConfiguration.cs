using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Models
{
    public class SimulationConfiguration : BaseEntity
    {
        public int SimulationId { get; set; }
        public int Time { get; set; }
        public int MaxCalculationTime { get; set; }
        public int Lotsize { get; set; }
    }
}
