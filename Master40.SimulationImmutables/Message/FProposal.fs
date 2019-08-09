module FProposal

open Akka.Actor
open System
open FPostponed

    type public FProposal =
        {
            PossibleSchedule : int64 
            Postponed : FPostponed 
            ResourceAgent : IActorRef
            Key: Guid
        }

