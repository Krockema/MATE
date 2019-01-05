using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Workcenter
    {
        public string WorkcenterId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public double? IdleTimePeriod { get; set; }
        public long? CapacityType { get; set; }
        public double? AllocationMax { get; set; }
        public long? ParallelSchedulingType { get; set; }
        public long? ParallelAllocationCriteria { get; set; }
        public double? FactorSpeed { get; set; }
        public long? InterruptionTimeMax { get; set; }
        public long? SetupmatrixDefaultTime { get; set; }
        public string SetupmatrixId { get; set; }
        public long? SetupStaticTimeNeedlessCriteria { get; set; }
        public long? SetupSchedulingType { get; set; }
        public long? SetupMandatoryOptimizationCriteria { get; set; }
        public long? MandatoryTimeInterval { get; set; }
        public long? ScheduleWorker { get; set; }
        public double? LotSizeMin { get; set; }
        public double? LotSizeMax { get; set; }
        public string LotSizeUnitId { get; set; }
        public long? StablePeriod { get; set; }
    }
}
