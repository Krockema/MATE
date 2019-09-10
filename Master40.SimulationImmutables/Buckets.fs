namespace Master40.SimulationImmutables

open Master40.DB.DataModel
open Akka.Actor
open System.Linq
open System
open Microsoft.FSharp.Collections


module Buckets = 
    type public FBuckets =
        { Key : Guid
          EstimatedStart : int64
          EstimatedEnd : int64
          //public double Priority { get; set; }
          PrioRule :  FSharpFunc<FBuckets, FSharpFunc<int64, double>>
          mutable ItemPriority : double
          DueTime : int64
          //Priority : double
          ResourceAgent : IActorRef
          HubAgent : IActorRef
          Status : ElementStatus
          Operations : Set<FWorkItem>
          MaxBucketSize : double
          MinBucketSize : double
          Proposals : System.Collections.Generic.List<FProposal> 
          WasSetReady : bool
          } interface IKey with 
                member this.Key  with get() = this.Key
        // Returns new Object with Updated Due
        member this.Priority time = this.ItemPriority <- this.PrioRule this time // Recalculate ItemPriority
                                    this.ItemPriority                        // Return new Priority
        member this.UpdateStatus s = { this with Status = s }
        member this.UpdateResourceAgent r = { this with ResourceAgent = r }
        member this.UpdateHubAgent hub = { this with HubAgent = hub }
        member this.AddOperation op = { this with Operations = this.Operations.Add(op);
                                                  DueTime = if this.DueTime > op.DueTime then op.DueTime else this.DueTime
                                        }
        member this.RemoveOperation op = { this with Operations = this.Operations.Remove(op);
                                                     DueTime = this.Operations.Min(fun y -> y.DueTime)        
                                        }
        member this.UpdateDueTime = { this with DueTime = this.Operations.Min(fun y -> y.DueTime)}
        member this.SetReady = { this with Status = ElementStatus.Ready; WasSetReady = true }
        member this.UpdateEstimations estimatedStart resourceAgent = { this with EstimatedEnd = estimatedStart + (int64)(this.Operations.Sum(fun x -> x.Operation.Duration));
                                                                                     EstimatedStart = (int64)estimatedStart;
                                                                                     ResourceAgent = resourceAgent } 
