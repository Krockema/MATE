module FUpdateSimulationWorkProvider

open Akka.Actor

    type public FUpdateSimulationWorkProvider = {
        ProductionAgents : System.Collections.Generic.List<IActorRef>
        RequestAgentId : string
        RequestAgentName : string
        IsHeadDemand : bool
        CustomerOrderId : int   
    }
