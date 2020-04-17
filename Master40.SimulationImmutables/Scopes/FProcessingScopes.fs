module FProcessingScopes

open IScopes

    type public FProcessingScope = 
        {   Start : int64
            End : int64 }
            interface IScope with
                member this.Start with get() = this.Start
                member this.End with get() = this.End
