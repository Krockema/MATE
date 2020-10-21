using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkergroup
    {
        public string ClientId { get; set; }
        public string WorkergroupId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public double? AllocationMax { get; set; }
        public int? CapacityType { get; set; }
        public double? CostRate { get; set; }
        public int PoolType { get; set; }
        public double? ProcessingTimePenalty { get; set; }
        public double? SetupTimePenalty { get; set; }
        public DateTime? LastModified { get; set; }
        public string GlobalCalendarId { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
