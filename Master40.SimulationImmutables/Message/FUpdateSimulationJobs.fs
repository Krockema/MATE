module FUpdateSimulationJobs

open IJobs

    type public FUpdateSimulationJob = {   
        Job : IJob
        JobType : string
        Duration : int64
        Start : int64
        Resource : string
        Bucket : string
        SetupId : int32
    } 

