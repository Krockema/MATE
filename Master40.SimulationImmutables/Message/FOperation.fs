module FOperations

open System
open Akka.Actor
open Master40.DB.DataModel
open FStartConditions
open IKeys
open IJobs
    [<CustomEquality;CustomComparison>] 
    type public FOperation =
        { Key : Guid
          DueTime : int64 
          CreationTime : int64
          BackwardEnd : int64 
          BackwardStart : int64 
          ForwardEnd : int64 
          ForwardStart : int64 
          RemainingWork : int64 
          CustomerDue : int64
          Start : int64
          End : int64 
          mutable StartConditions : FStartCondition
          Priority : int64 -> double
          ProductionAgent : IActorRef
          mutable HubAgent : IActorRef
          mutable IsFinished : bool 
          Operation : M_Operation
          RequiredCapability : M_ResourceCapability
          mutable SetupKey : int32
          Bucket : string
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
                member this.Start with get() = this.Start
                member this.StartConditions with get() = this.StartConditions
                member this.Priority time = this.Priority time 
                member this.SetupKey with get() = this.SetupKey
                member this.HubAgent with get() = this.HubAgent
                member this.RequiredCapability with get() = this.RequiredCapability
                member this.Duration = (int64)this.Operation.Duration // Theoretisch muss hier die Slacktime noch rein also , +3*duration bzw aus dem operationElement
                member this.UpdateEstimations estimatedStart = { this with End = estimatedStart +  (int64)this.Operation.Duration;
                                                                                         Start = (int64)estimatedStart; } :> IJob
                member this.Bucket with get() = this.Bucket
                member this.ResetSetup() = this.SetupKey <- -1
                member this.UpdateBucket bucketId = { this with Bucket = bucketId} :> IJob
            interface IComparable with 
                member this.CompareTo fWorkItem = 
                    match fWorkItem with 
                    | :? FOperation as other -> compare other.Key this.Key
                    | _ -> invalidArg "Operation" "cannot compare value of different types" 
                
        override this.Equals(other) =
            match other with
            | :? FOperation as operation -> this.Key.Equals(operation.Key)
            | _ -> invalidArg "Operation" "cannot compare value of different types" 
        override this.GetHashCode() = this.Key.GetHashCode()
        member this.SetFinished() = this.IsFinished <- true
        member this.UpdatePoductionAgent p = { this with ProductionAgent = p }
        member this.AsIjob = this :> IJob
        member this.UpdateHubAgent hub =  this.HubAgent <- hub 
        member this.SetStartConditions preCondition articleProvided = this.StartConditions <- { PreCondition = preCondition; ArticlesProvided = articleProvided } 
        member this.SetForwardSchedule earliestStart = { this with ForwardStart = earliestStart;
                                                                   ForwardEnd = earliestStart + (int64)this.Operation.Duration; }
        member this.UpdateCustomerDue due = { this with CustomerDue = due }
        member this.UpdateBucket bucketId = { this with Bucket = bucketId}