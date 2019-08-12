module FHubInformations

open Akka.Actor
open FResourceTypes

    type public FHubInformation = 
        {
            FromType : FResourceType
            RequiredFor : string
            Ref : IActorRef
        } with member this.UpdateRef r = { this with Ref = r }
