module FResourceInformations

open Akka.Actor
open Master40.DB.DataModel

    type public FResourceInformation = 
        {
            ResourceId : int32
            ResourceCapabilityProvider : System.Collections.Generic.List<M_ResourceCapabilityProvider>
            RequiredFor : string
            Ref : IActorRef
        } with member this.UpdateRef r = { this with Ref = r }