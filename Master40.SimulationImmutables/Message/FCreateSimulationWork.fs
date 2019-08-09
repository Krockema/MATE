module FCreateSimulationWork

open FOperation

    type public FCreateSimulationWork = {
        Operation : FOperation
        CustomerOrderId : string
        IsHeadDemand : bool
        ArticleType : string
    }
