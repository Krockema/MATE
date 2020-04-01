module FResourceInformations

open Akka.Actor
open Master40.DB.DataModel

    type public FResourceInformation = 
        {
            ResourceSetups : System.Collections.Generic.List<M_ResourceSetup>
            RequiredFor : string
            Ref : IActorRef
        } with member this.UpdateRef r = { this with Ref = r }