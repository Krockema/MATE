module IKeys

open System

type public IKey = 
    abstract member Key : Guid with get
    abstract member CreationTime : int64 with get