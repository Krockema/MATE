module FProposals

open Akka.Actor
open System
open FPostponeds

    type public FProposal =
        {
            PossibleSchedule : obj 
            Postponed : FPostponed
            CapabilityProviderId : int32
            ResourceAgent : obj
            JobKey: Guid
        }

