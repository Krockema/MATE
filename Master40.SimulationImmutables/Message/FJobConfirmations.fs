module FJobConfirmations

open IJobs
open Master40.DB.DataModel
open FQueueingPositions

    type public FJobConfirmation = {
        Job : IJob
        QueueingPosition : FQueueingPosition
        Duration : int64
        CapabilityProvider : M_ResourceCapabilityProvider
    } with member this.UpdateJob job = { this with Job = job }
           member this.IsReset = this.QueueingPosition.Equals(null)