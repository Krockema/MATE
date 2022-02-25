using System;
using Mate.DataCore.Interfaces;
using Mate.DataCore.Nominal;

namespace Mate.DataCore.ReportingModel
{
    public class SimulationOrder : ResultBaseEntity, IOrder
    {
        public string Name { get; set; }
        public int OriginId { get; set; }
        public int DueTime { get; set; }
        public int CreationTime { get; set; }
        public long ReleaseTime { get; set; }
        public int ProductionFinishedTime { get; set; }
        public int FinishingTime { get; set; }
        public int BusinessPartnerId { get; set; }
        public Guid StockExchangeGuid { get; set; }
        public State State { get; set; }
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
    }
}
