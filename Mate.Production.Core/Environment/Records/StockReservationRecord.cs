using System;

namespace Mate.Production.Core.Environment.Records
{
    public record StockReservationRecord
        (int Quantity, bool IsPurchased, bool IsInStock, DateTime DueTime, Guid TrackingId);
}