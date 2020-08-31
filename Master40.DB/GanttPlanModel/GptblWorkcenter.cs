using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkcenter
    {
        public string ClientId { get; set; }
        public string WorkcenterId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public double? AllocationMax { get; set; }
        public int? CapacityType { get; set; }
        public int? InterruptionTimeMax { get; set; }
        public double? FactorSpeed { get; set; }
        public double? IdleTimePeriod { get; set; }
        public double? LotSizeMax { get; set; }
        public double? LotSizeMin { get; set; }
        public string LotSizeUnitId { get; set; }
        public int? MandatoryTimeInterval { get; set; }
        public int? ParallelAllocationCriteria { get; set; }
        public int? ParallelSchedulingType { get; set; }
        public int? ScheduleWorker { get; set; }
        public int? SetupMandatoryOptimizationCriteria { get; set; }
        public int? SetupSchedulingType { get; set; }
        public int? SetupStaticTimeNeedlessCriteria { get; set; }
        public int? SetupmatrixDefaultTime { get; set; }
        public string SetupmatrixId { get; set; }
        public int? StablePeriod { get; set; }
        public DateTime? LastModified { get; set; }
        public double? SetupmatrixDefaultCosts { get; set; }
        public string GlobalCalendarId { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
