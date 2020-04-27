module FJobConfirmations

open IJobs
open Master40.DB.DataModel
open FScopeConfirmations
open Akka.Actor
open IConfirmations
open System

    type public FJobConfirmation = 
        { Job : IJob
          ScopeConfirmation : FScopeConfirmation
          Key : Guid
          Duration : int64
          CapabilityProvider : M_ResourceCapabilityProvider
          JobAgentRef : IActorRef
        } interface IConfirmation with
                member this.Key  with get() = this.Key
                member this.Job with get() = this.Job  
                member this.Duration with get() = this.Duration
                member this.ScopeConfirmation with get() = this.ScopeConfirmation
                member this.JobAgentRef with get() = this.JobAgentRef
                member this.CapabilityProvider with get() = this.CapabilityProvider
                member this.IsReset = this.ScopeConfirmation.Equals(null)
                member this.UpdateJob job = { this with Job = job } :> IConfirmation
        member this.UpdateScopeConfirmation scopeConfirmations = {this with ScopeConfirmation = scopeConfirmations }