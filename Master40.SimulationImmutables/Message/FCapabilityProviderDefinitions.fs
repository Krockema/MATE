module FCapabilityProviderDefinitions

open Master40.DB.Nominal.Model

    type public FCapabilityProviderDefinition = {
        WorkTimeGenerator : obj
        Resource : obj
        ResourceType : ResourceType
        CapabilityProvider : obj
        MaxBucketSize : int64
        TimeConstraintQueueLength : int32
        Debug : bool
    }


