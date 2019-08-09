module FStartConditions

    type public FStartConditions = 
        {
            mutable PreCondition : bool
            mutable MaterialsProvided : bool
        }
        member this.Satisfied = this.PreCondition && this.MaterialsProvided
