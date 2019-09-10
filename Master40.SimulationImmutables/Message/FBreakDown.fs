module FBreakDowns

    type public FBreakDown = {
        Resource : string
        ResourceSkill : string
        IsBroken : bool
        Duration : int64
    } with member this.SetIsBroken s = { this with IsBroken = s }
