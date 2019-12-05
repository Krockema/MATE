module FCreateSimulationResourceSetups

    type public FCreateSimulationResourceSetup = {
        Start : int64
        Duration : int64
        Resource : string
        ResourceTool : string
        ExpectedDuration : int64
    } 
