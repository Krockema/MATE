using Master40.DB.Enums;
using System;

namespace Master40.DB.Models
{
    public class StockExchange : BaseEntity
    {
        public const string STOCK_FEKY = "Stock";
        public int StockId { get; set; }
        public Guid TrakingGuid { get; set; }
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public Stock Stock { get; set; }
        public int RequiredOnTime { get; set; }
        public State State { get; set; }
        public decimal Quantity { get; set; }
        public int Time { get; set; }
        public ExchangeType ExchangeType { get; set; }
        
    }
}
