module FJobConfirmations

open IJobs
open Master40.DB.DataModel

    type public FJobConfirmation = {
        Job : IJob
        Schedule : int64
        Duration : int64
        CapabilityProvider : M_ResourceCapabilityProvider
    } with member this.UpdateJob job = { this with Job = job }
           member this.IsReset = this.Schedule.Equals(-1)