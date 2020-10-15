namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingOperation
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public int? AllowInterruptions { get; set; }
        public string Description { get; set; }
        public int? OperationType { get; set; }
        public int? SplitMax { get; set; }
        public int? SplitMin { get; set; }
        public int? WorkerRequirementType { get; set; }
        public double? QuantityRoundingValue { get; set; }
    }
}
