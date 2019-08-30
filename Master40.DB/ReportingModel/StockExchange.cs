using System;
using Master40.DB.Enums;
using Master40.DB.Interfaces;

namespace Master40.DB.ReportingModel
{
    public class StockExchange : BaseEntity, IStockExchange
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
