module FCapabilityProviderDefinitions

open Mate.DataCore.Nominal.Model

    type public FCapabilityProviderDefinition = {
        WorkTimeGenerator : obj
        Resource : obj
        ResourceType : ResourceType
        CapabilityProvider : obj
        MaxBucketSize : int64
        TimeConstraintQueueLength : int64
        Debug : bool
    }


