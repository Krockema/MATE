module FJobConfirmations

open IJobs
open Master40.DB.DataModel
open FScopeConfirmations
open Akka.Actor

    type public FJobConfirmation = {
        Job : IJob
        ScopeConfirmation : FScopeConfirmation
        Duration : int64
        CapabilityProvider : M_ResourceCapabilityProvider
        JobAgentRef : IActorRef
    } with member this.UpdateJob job = { this with Job = job }
           member this.IsReset = this.ScopeConfirmation.Equals(null)