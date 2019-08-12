module FStartConditions

    type public FStartCondition = 
        {
            mutable PreCondition : bool
            mutable MaterialsProvided : bool
        }
        member this.Satisfied = this.PreCondition && this.MaterialsProvided
