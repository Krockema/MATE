module FPostponeds

type public FPostponed =
    {
       Offset : int64
    }
    member this.IsPostponed = this.Offset <> 0L
    
