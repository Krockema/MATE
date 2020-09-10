module FCentralActivities

open Akka.Actor

    type public FCentralActivity = {
        ResourceId : string
        ProductionOrderId: string
        OperationId: string
        Duration: int64
        Name: string
        ActivityId: int
        GanttPlanningInterval: int
        Hub: IActorRef
    }