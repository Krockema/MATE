using System;
using Master40.DB.Enums;

namespace Master40.DB.Interfaces
{
    public interface IStockExchange
    {
        int StockId { get; set; }
        Guid TrackingGuid { get; set; }
        int SimulationConfigurationId { get; set; }
        SimulationType SimulationType { get; set; }
        int SimulationNumber { get; set; }
        int RequiredOnTime { get; set; }
        State State { get; set; }
        decimal Quantity { get; set; }
        int Time { get; set; }
        ExchangeType ExchangeType { get; set; }
        int Id { get; set; }
    }
}