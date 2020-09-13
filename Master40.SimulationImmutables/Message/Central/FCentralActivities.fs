module FCentralActivities

open Akka.Actor

    type public FCentralActivity = {
        ResourceId : string
        ProductionOrderId: string
        OperationId: string
        ActivityId: int
        Duration: int64
        Name: string
        GanttPlanningInterval: int
        Hub: IActorRef
    }