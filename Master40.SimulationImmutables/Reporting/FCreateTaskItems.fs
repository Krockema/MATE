module FCreateTaskItems

    type public FCreateTaskItem = {
        Type : string
        Resource : string
        Start: int64
        End: int64 
        Capability : string
        Operation : string
        GroupId : string
    }
