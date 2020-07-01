module FConfirmationSlots

open ITimeRanges
open System.Linq

    type public FConfirmationSlot = 
        {   IsQueueAble : bool
            IsRequieringSetup : bool
            Scopes : List<ITimeRange>
         } 
            member this.GetScopeStart() = this.Scopes.Min(fun y -> y.Start)
            member this.GetScopeEnd() = this.Scopes.Max(fun y -> y.End)