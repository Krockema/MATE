module FRequestResources

open Akka.Actor
open FResourceTypes

    type public FRequestResource =
        {
            Discriminator : string
            ResourceType : FResourceType
            actorRef : IActorRef
        }