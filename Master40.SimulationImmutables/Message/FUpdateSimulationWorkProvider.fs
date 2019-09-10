module FUpdateSimulationWorkProviders

open Akka.Actor
open System

    type public FUpdateSimulationWorkProvider = {
        FArticleProviderKeys : System.Collections.Generic.List<Guid>
        RequestAgentId : string
        RequestAgentName : string
        IsHeadDemand : bool
        CustomerOrderId : int   
    }
