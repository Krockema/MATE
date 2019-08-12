module IJobResults

open Akka.Actor

type public IJobResult = 
    abstract member Start : int64 with get
    abstract member End : int64 with get
    abstract member ResourceAgent : IActorRef with get

