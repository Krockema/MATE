module FBucketToRequeues

open FBuckets
open FOperations

type public FBucketToRequeue ={
        Bucket : FBucket
        Operation : FOperation
    }