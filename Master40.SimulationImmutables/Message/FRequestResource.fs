module FRequestResource

open Akka.Actor
open FResourceType

    type public FRequestResource =
        {
            Discriminator : string
            ResourceType : FResourceType
            actorRef : IActorRef
        }