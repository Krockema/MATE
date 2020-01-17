module FJobInformations

open IJobs
open Master40.DB.DataModel

    type public FJobInformation = {   
        Job : IJob
        Resouce : string
        Tool : string
        Setup : M_ResourceSetup
        Bucket : string
    } 

