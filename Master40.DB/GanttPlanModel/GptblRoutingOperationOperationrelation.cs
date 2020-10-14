namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingOperationOperationrelation
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string SuccessorOperationId { get; set; }
        public string SuccessorAlternativeId { get; set; }
        public int SuccessorSplitId { get; set; }
        public int? OverlapType { get; set; }
        public double? OverlapValue { get; set; }
        public int? TimeBufferMax { get; set; }
        public int? TimeBufferMin { get; set; }
    }
}
