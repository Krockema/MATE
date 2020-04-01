module FResourceInformations

open Akka.Actor
open Master40.DB.DataModel

    type public FResourceInformation = 
        {
            ResourceCapabilities : System.Collections.Generic.List<M_ResourceCapability>
            RequiredFor : string
            Ref : IActorRef
        } with member this.UpdateRef r = { this with Ref = r }