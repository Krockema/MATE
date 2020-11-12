module FQueuingJobs

open Akka.Actor;
open System;
open Master40.DB.DataModel;
open IQueueingJobs;
open IJobs;

    type public FQueuingJob = 
        {
            Key : Guid
            ResourceId : int
            Job : IJob
            JobKey : Guid
            JobName : string
            Duration : int64
            Hub: IActorRef
            CapabilityProvider : M_ResourceCapabilityProvider
            JobType : string
        } interface IQueueingJob with
            member this.Key  with get() = this.Key
            member this.ResourceId with get() = this.ResourceId
            member this.Job with get() = this.Job
            member this.JobKey with get() = this.JobKey
            member this.JobName with get() = this.JobName
            member this.Duration with get() = this.Duration
            member this.Hub with get() = this.Hub
            member this.CapabilityProvider with get() = this.CapabilityProvider
            member this.JobType with get() = this.JobType

        