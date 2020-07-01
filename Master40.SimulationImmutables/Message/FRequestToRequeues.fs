module FRequestToRequeues

open System

type public FRequestToRequeue = 
    {
        JobKey : Guid
        Approved : bool
    }
    member this.SetApproved = { this with Approved = true}