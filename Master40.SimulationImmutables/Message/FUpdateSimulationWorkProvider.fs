module FUpdateSimulationWorkProviders

open Akka.Actor
open System
open FStockProviders

    type public FUpdateSimulationWorkProvider = {
        FArticleProviderKeys : System.Collections.Generic.List<FStockProvider>
        RequestAgentId : string
        RequestAgentName : string
        IsHeadDemand : bool
        CustomerOrderId : int   
    }
