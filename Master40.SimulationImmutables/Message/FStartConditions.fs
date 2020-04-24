module FStartConditions

    type public FStartCondition = 
        {
            PreCondition : bool
            ArticlesProvided : bool
        }
        member this.Satisfied = this.PreCondition && this.ArticlesProvided