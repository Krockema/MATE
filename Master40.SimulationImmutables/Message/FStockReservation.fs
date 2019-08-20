module FStockReservations

open System

type public FStockReservation =
    {
        Quantity : int
        IsPurchased : bool
        IsInStock : bool
        DueTime : int64
        TrackingId : Guid
    }
