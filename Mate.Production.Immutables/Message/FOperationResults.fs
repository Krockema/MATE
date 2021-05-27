module FOperationResults

open System
open Akka.Actor
open IKeys
open IJobResults

    type public FOperationResult =
        { Key : Guid
          CreationTime : int64
          Start : int64
          End : int64 
          OriginalDuration : int64
          ProductionAgent : IActorRef
          CapabilityProvider : String
          } interface IKey with
                member this.Key  with get() = this.Key
                member this.CreationTime with get() = this.CreationTime
            interface IJobResult with
                member this.Key with get() = this.Key
                member this.End with get() = this.End
                member this.Start with get() = this.Start
                member this.CapabilityProvider with get() = this.CapabilityProvider
                member this.OriginalDuration with get() = this.OriginalDuration
                member this.FinishedAt f = { this with End = f;
                                                       OriginalDuration = this.End - this.Start } :> IJobResult
        member this.UpdatePoductionAgent p = { this with ProductionAgent = p }