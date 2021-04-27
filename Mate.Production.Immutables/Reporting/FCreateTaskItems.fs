module FCreateTaskItems

    type public FCreateTaskItem = {
        Type : string
        Resource : string
        ResourceId : int
        Start: int64
        End: int64 
        Capability : string
        Operation : string
        GroupId : int64
    }
