module FSetupDefinitions

open Akka.Actor

type FSetupDefinition = {
    SetupKey : int32
    RequiredResources : System.Collections.Generic.List<IActorRef>
}