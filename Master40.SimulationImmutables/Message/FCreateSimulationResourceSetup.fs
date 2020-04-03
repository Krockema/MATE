module FCreateSimulationResourceSetups

    type public FCreateSimulationResourceSetup = {
        Start : int64
        Duration : int64
        Resource : string
        CapabilityName : string
        ExpectedDuration : int64
    } 
