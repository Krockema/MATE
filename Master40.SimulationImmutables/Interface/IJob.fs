module IJobs

open FStartConditions
open FProposals
open Akka.Actor

type public IJob = 
    abstract member ForwardStart : int64 with get
    abstract member ForwardEnd : int64 with get
    abstract member BackwardStart : int64 with get
    abstract member BackwardEnd : int64 with get
    abstract member Start : int64 with get
    abstract member End : int64 with get
    abstract member StartConditions : FStartCondition with get
    abstract member Priority : int64 -> double 
    abstract member Proposals : System.Collections.Generic.List<FProposal> 
    abstract member ResourceAgent : IActorRef
    abstract member HubAgent : IActorRef
    abstract member DueTime : int64 with get
    abstract member Duration : int64 with get
    abstract member SetEstimatedEnd : int64 -> IJob
