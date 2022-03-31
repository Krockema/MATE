module FCreateTaskItems

    type public FCreateTaskItem = {
        Type : string
        Resource : string
        ResourceId : int
        ReadyAt : int64
        Start: int64
        End: int64 
        Capability : string
        Operation : string
        GroupId : int64
    }
