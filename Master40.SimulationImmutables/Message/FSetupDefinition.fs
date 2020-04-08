module FSetupDefinitions

open Akka.Actor

type FSetupDefinition = {
    SetupKey : int32 // TODO
    RequiredResources : System.Collections.Generic.List<IActorRef>
}