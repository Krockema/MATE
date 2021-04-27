using System;
using Mate.DataCore.Interfaces;
using Mate.DataCore.Nominal;

namespace Mate.DataCore.ReportingModel
{
    public class StockExchange : ResultBaseEntity, IStockExchange
    {
        public int StockId { get; set; }
        public Guid TrackingGuid { get; set; }
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public string Stock { get; set; }
        public int RequiredOnTime { get; set; }
        public State State { get; set; }
        public decimal Quantity { get; set; }
        public int Time { get; set; }
        public ExchangeType ExchangeType { get; set; }
        
    }
}
