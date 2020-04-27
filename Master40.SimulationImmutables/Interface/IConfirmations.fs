module IConfirmations

open System
open IJobs
open FScopeConfirmations
open Master40.DB.DataModel
open Akka.Actor

type public IConfirmation = 
    abstract member Job : IJob with get
    abstract member Key : Guid with get
    abstract member Duration : int64 with get
    abstract member ScopeConfirmation : FScopeConfirmation with get
    abstract member CapabilityProvider: M_ResourceCapabilityProvider with get
    abstract member JobAgentRef : IActorRef with get
    abstract member IsReset: bool with get
    abstract member UpdateJob : IJob -> IConfirmation