module FOperation

open System
open Akka.Actor
open Master40.DB.DataModel
open FProposal
open FStartConditions
open IKey
open IJob

    type public FOperation =
        { Key : Guid
          DueTime : int64 
          CreationTime : int64
          BackwardEnd : int64 
          BackwardStart : int64 
          End : int64 
          ForwardEnd : int64 
          ForwardStart : int64 
          Start : int64
          Priority : int64
          StartConditions : FStartConditions
          PrioRule :  FSharpFunc<int64, double> 
          ProductionAgent : IActorRef
          ResourceAgent : IActorRef
          HubAgent : IActorRef
          Operation : M_Operation
          Proposals : System.Collections.Generic.List<FProposal> 
          } interface IKey with
                member this.Key  with get() = this.Key
                member this.DueTime with get() = this.DueTime
                member this.CreationTime with get() = this.CreationTime
            interface IJob with
                member this.BackwardEnd with get() = this.BackwardEnd
                member this.BackwardStart with get() = this.BackwardStart
                member this.End with get() = this.End
                member this.ForwardEnd with get() = this.ForwardEnd
                member this.ForwardStart with get() = this.ForwardStart
                member this.Proposals with get() = this.Proposals
                member this.Start with get() = this.Start
                member this.StartConditions with get() = this.StartConditions
                member this.Priority time = this.PrioRule(time)
                member this.ResourceAgent with get() = this.ResourceAgent
                member this.HubAgent with get() = this.HubAgent
            interface IComparable with 
                member this.CompareTo fWorkItem = 
                    match fWorkItem with 
                    | :? FOperation as other -> compare other.Key this.Key
                    | _ -> invalidArg "Operation" "cannot compare value of different types" 
        member this.UpdatePoductionAgent p = { this with ProductionAgent = p }
        member this.UpdateResourceAgent r = { this with ResourceAgent = r }
        member this.UpdateHubAgent hub = { this with HubAgent = hub }