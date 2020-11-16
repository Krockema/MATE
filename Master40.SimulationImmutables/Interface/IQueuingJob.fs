module IQueueingJobs
open Akka.Actor
open System
open IJobs
open Master40.DB.DataModel

type public IQueueingJob = 
    abstract member Key : Guid with get
    abstract member ResourceId : int with get
    abstract member Job : IJob with get
    abstract member JobKey : Guid with get
    abstract member JobName : string with get
    abstract member Duration : int64 with get
    abstract member Hub : IActorRef with get
    abstract member CapabilityProvider : M_ResourceCapabilityProvider with get
    abstract member JobType : string with get