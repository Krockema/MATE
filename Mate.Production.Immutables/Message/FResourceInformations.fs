module FResourceInformations

open Akka.Actor
open Mate.DataCore.DataModel

    type public FResourceInformation = 
        {
            ResourceId : int32
            ResourceName : string
            ResourceCapabilityProvider : System.Collections.Generic.List<M_ResourceCapabilityProvider>
            ResourceType : Mate.DataCore.Nominal.Model.ResourceType
            RequiredFor : string
            Ref : IActorRef
        } with member this.UpdateRef r = { this with Ref = r }