module FBreakDowns

    type public FBreakDown = {
        Resource : string
        ResourceCapability : string
        IsBroken : bool
        Duration : int64
    } with member this.SetIsBroken s = { this with IsBroken = s }
