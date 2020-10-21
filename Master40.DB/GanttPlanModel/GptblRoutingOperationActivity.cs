namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingOperationActivity
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public int ActivityId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public int? ActivityType { get; set; }
        public int? ConfirmationRequired { get; set; }
        public string Description { get; set; }
        public int? ParallelCountMax { get; set; }
        public double? TimeQuantityDependent { get; set; }
        public int? TimeQuantityIndependent { get; set; }
        public double? WorkcenterAllocation { get; set; }
        public int? InterruptionTimeMax { get; set; }
    }
}
