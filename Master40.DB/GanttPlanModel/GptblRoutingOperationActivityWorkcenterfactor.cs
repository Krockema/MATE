namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingOperationActivityWorkcenterfactor
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public int ActivityId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string WorkcenterId { get; set; }
        public double? FactorWorkcenterTime { get; set; }
    }
}
