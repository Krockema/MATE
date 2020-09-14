module FCentralResourceRegistrations

open Akka.Actor

    type public FCentralResourceRegistration = {
        ResourceId : string
        ResourceName: string
        ResourceActorRef: IActorRef
        ResourceGroupId : string
        ResourceType : int
    }