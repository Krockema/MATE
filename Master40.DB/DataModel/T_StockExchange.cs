using System;
using Master40.DB.Enums;
using Master40.DB.Interfaces;

namespace Master40.DB.DataModel
{
    public class T_StockExchange : BaseEntity, IStockExchange
    {
        public const string STOCK_FEKY = "Stock";
        public int StockId { get; set; }
        public Guid TrakingGuid { get; set; }
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public M_Stock Stock { get; set; }
        public int RequiredOnTime { get; set; }
        public State State { get; set; }
        public decimal Quantity { get; set; }
        public int Time { get; set; }
        public ExchangeType ExchangeType { get; set; }
        
    }
}
