module IJobResults

open Akka.Actor
open System

type public IJobResult = 
    abstract member Key : Guid with get
    abstract member Start : int64 with get
    abstract member End : int64 with get
    abstract member ResourceAgent : IActorRef with get
    abstract member OriginalDuration : int64 with get
    abstract member FinishedAt : int64 -> IJobResult

