using System;
using Mate.DataCore.Nominal;

namespace Mate.DataCore.Interfaces
{
    public interface IStockExchange
    {
        int StockId { get; set; }
        Guid TrackingGuid { get; set; }
        int SimulationConfigurationId { get; set; }
        SimulationType SimulationType { get; set; }
        int SimulationNumber { get; set; }
        long RequiredOnTime { get; set; }
        State State { get; set; }
        decimal Quantity { get; set; }
        long Time { get; set; }
        ExchangeType ExchangeType { get; set; }
        int Id { get; set; }
    }
}