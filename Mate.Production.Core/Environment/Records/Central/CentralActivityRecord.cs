using Akka.Actor;
using System;

public record CentralActivityRecord(int ResourceId
    , string ProductionOrderId
    , string OperationId
    , int ActivityId
    , DateTime Start
    , TimeSpan Duration
    , string Name
    , int GanttPlanningInterval
    , IActorRef Hub
    , string Capability
    , string ActivityType) 
    {
        public string Key => this.ProductionOrderId + "|" + this.OperationId + "|" + this.ActivityId.ToString();
    }