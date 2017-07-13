using System;
using Master40.DB.Enums;

namespace Master40.DB.Models
{
    public class Simulation : BaseEntity
    {
        public string SimulationId { get; set; }
        public string SimulationDbState { get; set; }
        public SimulationType SimulationType { get; set; }
        public DateTime CreationDate { get; set; }
    }
}