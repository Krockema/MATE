module FFinishOperations

open IJobs
open Mate.DataCore.DataModel

    type public FFinishOperation =
        { Job : IJob
          Duration : int64 
          CapabilityProvider : M_ResourceCapabilityProvider
          BucketName : string 
          } 