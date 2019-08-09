module FPostponed

type public FPostponed =
    {
        PostponedFor : int64
    }
    member this.Postponed = this.PostponedFor <> 0L
    
