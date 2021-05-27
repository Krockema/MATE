module FProductionResults
open Akka.Actor
open System
open IKeys

    type public FProductionResult =
        { Key : Guid
          TrackingId : Guid
          CreationTime : int64
          CustomerDue : int64
          ProductionRef : IActorRef
          Amount : decimal
        } interface IKey with
                member this.Key  with get() = this.Key
                member this.CreationTime with get() = this.CreationTime
