using System.Collections.Generic;
using Master40.DB.Enums;
using Newtonsoft.Json;
using Master40.DB.Interfaces;

namespace Master40.DB.Models
{
    public class SimulationOrder : BaseEntity, IOrder
    {
        public string Name { get; set; }
        public int DueTime { get; set; }
        public int CreationTime { get; set; }
        public int FinishingTime { get; set; }
        public int BusinessPartnerId { get; set; }
        public State State { get; set; }
        public int SimulationConfigurationId { get; set; }
        public string SimulationType { get; set; }
        public int SimulationNumber { get; set; }
    }
}
