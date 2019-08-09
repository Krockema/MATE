module FHubInformation

open Akka.Actor
open FResourceType

    type public FHubInformation = 
        {
            FromType : FResourceType
            RequiredFor : string
            Ref : IActorRef
        } with member this.UpdateRef r = { this with Ref = r }
