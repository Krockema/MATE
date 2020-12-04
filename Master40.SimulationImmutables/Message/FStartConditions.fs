module FStartConditions

    type public FStartCondition = 
        {
            PreCondition : bool
            ArticlesProvided : bool
            WasSetReadyAt : int64
        }
        member this.Satisfied = this.PreCondition && this.ArticlesProvided