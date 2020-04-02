module FSetupDefinitions

open Akka.Actor

type FSetupDefinition = {
    SetupKey : int64
    RequiredResources : System.Collections.Generic.List<IActorRef>
}