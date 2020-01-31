module FCreateSimulationJobs

open System
open IJobs

    type public FCreateSimulationJob = {
        Job : IJob
        JobType : string
        CustomerOrderId : string
        IsHeadDemand : bool
        ArticleType : string
        fArticleKey : Guid
        ProductionAgent : string
        fArticleName: string
        Start : int64
        End : int64
    }
