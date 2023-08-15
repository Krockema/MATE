using System;
namespace Mate.Production.Core.Environment.Records.Central
{
    public record CentralStockDefinitionRecord(
        int StockId,
        string MaterialName,
        double InitialQuantity,
        string Unit,
        double Price,
        string MaterialType,
        TimeSpan DeliveryPeriod);
}

