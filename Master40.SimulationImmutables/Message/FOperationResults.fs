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
          ProductionAgent : IActorRef
          ResourceAgent : IActorRef
          } interface IKey with
                member this.Key  with get() = this.Key
                member this.CreationTime with get() = this.CreationTime
            interface IJobResult with
                member this.End with get() = this.End
                member this.Start with get() = this.Start
                member this.ResourceAgent with get() = this.ResourceAgent
        member this.UpdatePoductionAgent p = { this with ProductionAgent = p }
        member this.UpdateResourceAgent r = { this with ResourceAgent = r }