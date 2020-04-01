module FProposals

open Akka.Actor
open System
open FPostponeds

    type public FProposal =
        {
            PossibleSchedule : int64 
            Postponed : FPostponed
            SetupId : int64
            ResourceAgent : IActorRef
            JobKey: Guid
        }

