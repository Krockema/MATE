module IJobResults

open System

type public IJobResult = 
    abstract member Key : Guid with get
    abstract member Start : int64 with get
    abstract member End : int64 with get
    abstract member CapabilityProvider : String with get
    abstract member OriginalDuration : int64 with get
    abstract member FinishedAt : int64 -> IJobResult

