module FCentralActivities
    type public FCentralActivity = {
        ResourceId : string
        ProductionOrderId: string
        OperationId: string
        ActivityId: int
        GanttPlanningInterval: int
    }