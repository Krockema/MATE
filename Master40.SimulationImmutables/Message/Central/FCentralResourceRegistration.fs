module FCentralResourceRegistrations

open Akka.Actor

    type public FCentralResourceRegistration = {
        ResourceId : int
        ResourceName: string
        ResourceActorRef: IActorRef
        ResourceGroupId : string
        ResourceType : int
    }