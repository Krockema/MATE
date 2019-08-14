module FAgentInformations

open Akka.Actor
open FResourceTypes

    type public FAgentInformation = 
        {
            FromType : FResourceType
            RequiredFor : string
            Ref : IActorRef
        } with member this.UpdateRef r = { this with Ref = r }
