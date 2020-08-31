namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingOperationWorkcenter
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string WorkcenterId { get; set; }
        public double? Delta { get; set; }
        public double? LotSizeMax { get; set; }
        public double? LotSizeMin { get; set; }
    }
}
