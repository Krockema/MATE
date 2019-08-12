module FStockReservations

open System

type public FStockReservation =
    {
        Quantity : int
        IsPurchsed : bool
        IsInStock : bool
        DueTime : int64
        TrackingId : Guid
    }
