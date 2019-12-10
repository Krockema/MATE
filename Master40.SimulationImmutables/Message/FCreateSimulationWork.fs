module FCreateSimulationWorks

open FOperations
open System

    type public FCreateSimulationWork = {
        Operation : FOperation
        CustomerOrderId : string
        IsHeadDemand : bool
        ArticleType : string
        fArticleKey : Guid
        fArticleName: string
    }
