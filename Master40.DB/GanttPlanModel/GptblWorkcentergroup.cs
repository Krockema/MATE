using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkcentergroup
    {
        public string ClientId { get; set; }
        public string WorkcentergroupId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public double? AllocationMax { get; set; }
        public int? CapacityType { get; set; }
        public double? IdleTimePeriod { get; set; }
        public int? MandatoryTimeInterval { get; set; }
        public int? ParallelAllocationCriteria { get; set; }
        public int? ParallelSchedulingType { get; set; }
        public int? LineType { get; set; }
        public DateTime? LastModified { get; set; }
        public string GlobalCalendarId { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
