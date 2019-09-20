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
        fArticleName: string
    }
