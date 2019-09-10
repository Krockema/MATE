module FStartConditions

    type public FStartCondition = 
        {
            mutable PreCondition : bool
            mutable ArticlesProvided : bool
        }
        member this.Satisfied = this.PreCondition && this.ArticlesProvided