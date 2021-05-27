module FBucketScopes

open System

type public FBucketScope = {
        BucketKey : Guid
        Start : int64
        End : int64
        Duration : int64
}


