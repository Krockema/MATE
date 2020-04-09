module FProposals

open Akka.Actor
open System
open FPostponeds

    type public FProposal =
        {
            PossibleSchedule : int64 
            Postponed : FPostponed
            CapabilityProviderId : int32
            ResourceAgent : IActorRef
            JobKey: Guid
        }

