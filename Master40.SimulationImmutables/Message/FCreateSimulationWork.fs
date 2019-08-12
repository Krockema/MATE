module FCreateSimulationWorks

open FOperations

    type public FCreateSimulationWork = {
        Operation : FOperation
        CustomerOrderId : string
        IsHeadDemand : bool
        ArticleType : string
    }
