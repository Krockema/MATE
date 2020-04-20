module FJobResourceConfirmations

open FJobConfirmations
open Akka.Actor
open FScopeConfirmations
open System.Collections.Generic

    type public FJobResourceConfirmation = {
        JobConfirmation : FJobConfirmation
        ScopeConfirmations : Dictionary<IActorRef,FScopeConfirmation>
    }

