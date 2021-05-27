module FQueueingScopes

open ITimeRanges
open System.Linq

    type public FQueueingScope = 
        {   IsQueueAble : bool
            IsRequieringSetup : bool
            Scope : ITimeRange
         } 