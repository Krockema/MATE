module FProposals
open Akka.Actor
open System
open FPostponeds

    type public FProposal =
        {
            PossibleSchedule : obj 
            Postponed : FPostponed
            CapabilityId : int32
            ResourceAgent : IActorRef
            JobKey: Guid
        }

