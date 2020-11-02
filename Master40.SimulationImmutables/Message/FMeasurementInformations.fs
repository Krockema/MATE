module FMeasurementInformations

open IJobs

    type public FMeasurementInformation = {   
        Job : IJob
        Resource : string
        Quantile: double
        Tool : string
        CapabilityProviderId : int
        Bucket : string
    } 

