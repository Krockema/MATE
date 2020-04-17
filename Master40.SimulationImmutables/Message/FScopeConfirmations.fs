module FScopeConfirmations

open ITimeRanges
open System.Linq
open FSetupSlots
open FProcessingSlots

    type public FScopeConfirmation =         
        {   
            Scopes : List<ITimeRange>
        } 
            member this.GetScopeStart() = this.Scopes.Min(fun y -> y.Start)
            member this.GetScopeEnd() = this.Scopes.Max(fun y -> y.End)
            member this.GetSetup() = this.Scopes.SingleOrDefault(fun y -> y.GetType().Equals(typeof<FSetupSlot>))
            member this.GetProcessing() = this.Scopes.SingleOrDefault(fun y -> y.GetType().Equals(typeof<FProcessingSlot>))