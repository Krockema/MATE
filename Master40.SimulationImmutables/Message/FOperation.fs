module FOperations

open System
open Akka.Actor
open Master40.DB.DataModel
open FProposals
open FStartConditions
open IKeys
open IJobs
open FUpdateStartConditions

    type public FOperation =
        { Key : Guid
          DueTime : int64 
          CreationTime : int64
          BackwardEnd : int64 
          BackwardStart : int64 
          ForwardEnd : int64 
          ForwardStart : int64 
          Start : int64
          End : int64 
          StartConditions : FStartCondition
          Priority : int64 -> double
          ProductionAgent : IActorRef
          ResourceAgent : IActorRef
          mutable HubAgent : IActorRef
          Operation : M_Operation
          Tool : M_ResourceTool
          Proposals : System.Collections.Generic.List<FProposal> 
          } interface IKey with
                member this.Key  with get() = this.Key
                member this.CreationTime with get() = this.CreationTime
            interface IJob with
                member this.Key  with get() = this.Key
                member this.Name with get() = this.Operation.Name
                member this.BackwardEnd with get() = this.BackwardEnd
                member this.BackwardStart with get() = this.BackwardStart
                member this.DueTime with get() = this.DueTime
                member this.End with get() = this.End
                member this.ForwardEnd with get() = this.ForwardEnd
                member this.ForwardStart with get() = this.ForwardStart
                member this.Proposals with get() = this.Proposals
                member this.Start with get() = this.Start
                member this.StartConditions with get() = this.StartConditions
                member this.Priority time = this.Priority time 
                member this.ResourceAgent with get() = this.ResourceAgent
                member this.HubAgent with get() = this.HubAgent
                member this.Tool with get() = this.Tool
                member this.Duration = (int64)this.Operation.Duration // Theoretisch muss hier die Slacktime noch rein also , +3*duration bzw aus dem operationElement
                member this.UpdateEstimations estimatedStart resourceAgent = { this with End = estimatedStart +  (int64)this.Operation.Duration;
                                                                                         Start = (int64)estimatedStart;
                                                                                         ResourceAgent = resourceAgent } :> IJob
            interface IComparable with 
                member this.CompareTo fWorkItem = 
                    match fWorkItem with 
                    | :? FOperation as other -> compare other.Key this.Key
                    | _ -> invalidArg "Operation" "cannot compare value of different types" 
        member this.UpdatePoductionAgent p = { this with ProductionAgent = p }  
        member this.AsIjob = this :> IJob
        member this.UpdateResourceAgent r = { this with ResourceAgent = r }
        member this.UpdateHubAgent hub =  this.HubAgent <- hub 
        member this.SetStartConditions(startCondition : FUpdateStartCondition) = this.StartConditions.ArticlesProvided <- startCondition.ArticlesProvided 
                                                                                 this.StartConditions.PreCondition <- startCondition.PreCondition
        member this.SetForwardSchedule earliestStart = { this with ForwardStart = earliestStart;
                                                                   ForwardEnd = earliestStart + (int64)this.Operation.Duration; }