using System;
using Mate.DataCore.Interfaces;
using Mate.DataCore.Nominal;

namespace Mate.DataCore.ReportingModel
{
    public class SimulationOrder : ResultBaseEntity, IOrder
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int OriginId { get; set; }
        public DateTime DueTime { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ProductionFinishedTime { get; set; }
        public DateTime FinishingTime { get; set; }
        public int BusinessPartnerId { get; set; }
        public Guid StockExchangeGuid { get; set; }
        public State State { get; set; }
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
    }
}
