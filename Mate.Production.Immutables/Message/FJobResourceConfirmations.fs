module FJobResourceConfirmations

open IConfirmations
open Akka.Actor
open FScopeConfirmations
open System.Collections.Generic

    type public FJobResourceConfirmation = {
        JobConfirmation : IConfirmation
        ScopeConfirmations : Dictionary<IActorRef,FScopeConfirmation>
    }

