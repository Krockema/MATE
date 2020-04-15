module FQueueingScopes

open IScopes

    type public FQueueingScope = 
        {   IsQueueAble : bool
            IsRequieringSetup : bool
            Start : int64
            End : int64
            EstimatedSetup : int64
            EstimatedWork : int64 }
                interface IScope with
                    member this.Start with get() = this.Start
                    member this.End with get() = this.End
                    member this.EstimatedWork with get() = this.EstimatedWork    
    