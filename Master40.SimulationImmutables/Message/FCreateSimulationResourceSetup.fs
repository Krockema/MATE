module FCreateSimulationResourceSetups

    type public FCreateSimulationResourceSetup = {
        Start : int64
        Duration : int64
        CapabilityProvider : string
        CapabilityName : string
        ExpectedDuration : int64
        SetupId: int32
    } 
